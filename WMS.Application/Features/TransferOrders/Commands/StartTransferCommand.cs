using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.TransferOrders.Commands;

public record StartTransferCommand(Guid Id) : IRequest<Result>;

public class StartTransferCommandHandler : IRequestHandler<StartTransferCommand, Result>
{
    private readonly ITransferOrderRepository _repository;
    private readonly IStockRepository _stockRepository;

    public StartTransferCommandHandler(
        ITransferOrderRepository repository,
        IStockRepository stockRepository)
    {
        _repository = repository;
        _stockRepository = stockRepository;
    }

    public async Task<Result> Handle(StartTransferCommand request, CancellationToken cancellationToken)
    {
        var to = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (to == null)
        {
            return Result.Failure($"Transfer order with ID '{request.Id}' was not found.");
        }

        if (to.Status != TransferOrderStatus.Approved)
        {
            return Result.Failure($"Only approved transfer orders can be started. Current status: {to.Status}");
        }

        // 1. Verify stock and reserve it
        foreach (var item in to.Items)
        {
            var stocks = await _stockRepository.FindAsync(s => s.ProductId == item.ProductId && s.LocationId == item.FromLocationId, cancellationToken);
            var stock = stocks.FirstOrDefault();
            if (stock == null)
            {
                return Result.Failure($"Stock for product '{item.Product.Code}' does not exist at the source location.");
            }

            var available = stock.Quantity - stock.ReservedQuantity;
            if (available < item.Qty)
            {
                return Result.Failure($"Insufficient stock for product '{item.Product.Code}' at source location. Required: {item.Qty}, Available: {available}.");
            }

            stock.ReservedQuantity += item.Qty;
            _stockRepository.Update(stock);
            
            item.Status = TransferOrderItemStatus.Shipped;
        }

        to.Status = TransferOrderStatus.InTransit;
        _repository.Update(to);

        return Result.Success();
    }
}
