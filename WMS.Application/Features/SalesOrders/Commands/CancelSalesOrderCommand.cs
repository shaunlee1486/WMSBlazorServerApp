using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.SalesOrders.Commands;

public record CancelSalesOrderCommand(Guid Id) : IRequest<Result>;

public class CancelSalesOrderCommandHandler : IRequestHandler<CancelSalesOrderCommand, Result>
{
    private readonly ISalesOrderRepository _repository;
    private readonly IStockRepository _stockRepository;
    private readonly IPickListRepository _pickListRepository;

    public CancelSalesOrderCommandHandler(
        ISalesOrderRepository repository,
        IStockRepository stockRepository,
        IPickListRepository pickListRepository)
    {
        _repository = repository;
        _stockRepository = stockRepository;
        _pickListRepository = pickListRepository;
    }

    public async Task<Result> Handle(CancelSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var so = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (so == null)
        {
            return Result.Failure($"Sales order with ID '{request.Id}' was not found.");
        }

        if (so.Status != SalesOrderStatus.Draft && so.Status != SalesOrderStatus.Confirmed)
        {
            return Result.Failure($"Only draft or confirmed sales orders can be cancelled. Current status: {so.Status}");
        }

        // 1. Release reservations if the sales order was Confirmed
        if (so.Status == SalesOrderStatus.Confirmed)
        {
            foreach (var item in so.Items)
            {
                var stocks = await _stockRepository.FindAsync(s => s.ProductId == item.ProductId, cancellationToken);
                var remainingToRelease = item.OrderedQty;

                foreach (var stock in stocks.Where(s => s.ReservedQuantity > 0).OrderByDescending(s => s.ReservedQuantity))
                {
                    var qtyToRelease = Math.Min(stock.ReservedQuantity, remainingToRelease);
                    stock.ReservedQuantity -= qtyToRelease;
                    remainingToRelease -= qtyToRelease;

                    _stockRepository.Update(stock);

                    if (remainingToRelease <= 0) break;
                }
            }

            // 2. Cancel any active Pick Lists linked to this Sales Order
            var pickListsResult = await _pickListRepository.FindAsync(
                pl => pl.SalesOrderId == so.Id && pl.Status != PickListStatus.Cancelled && pl.Status != PickListStatus.Completed,
                cancellationToken);

            foreach (var pl in pickListsResult)
            {
                pl.Status = PickListStatus.Cancelled;
                _pickListRepository.Update(pl);
            }
        }

        // 3. Mark Sales Order as Cancelled
        so.Status = SalesOrderStatus.Cancelled;
        _repository.Update(so);

        return Result.Success();
    }
}
