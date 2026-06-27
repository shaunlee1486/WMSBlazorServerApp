using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.TransferOrders.Commands;

public record CancelTransferCommand(Guid Id) : IRequest<Result>;

public class CancelTransferCommandHandler : IRequestHandler<CancelTransferCommand, Result>
{
    private readonly ITransferOrderRepository _repository;
    private readonly IStockRepository _stockRepository;

    public CancelTransferCommandHandler(
        ITransferOrderRepository repository,
        IStockRepository stockRepository)
    {
        _repository = repository;
        _stockRepository = stockRepository;
    }

    public async Task<Result> Handle(CancelTransferCommand request, CancellationToken cancellationToken)
    {
        var to = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (to == null)
        {
            return Result.Failure($"Transfer order with ID '{request.Id}' was not found.");
        }

        if (to.Status == TransferOrderStatus.Completed || to.Status == TransferOrderStatus.Cancelled)
        {
            return Result.Failure($"Cannot cancel transfer order in its current status: {to.Status}");
        }

        // If the transfer had already started, we need to release the reserved source stock
        if (to.Status == TransferOrderStatus.InTransit)
        {
            foreach (var item in to.Items)
            {
                var sourceStocks = await _stockRepository.FindAsync(s => s.ProductId == item.ProductId && s.LocationId == item.FromLocationId, cancellationToken);
                var sourceStock = sourceStocks.FirstOrDefault();
                if (sourceStock != null)
                {
                    sourceStock.ReservedQuantity -= item.Qty;
                    if (sourceStock.ReservedQuantity < 0) sourceStock.ReservedQuantity = 0; // clamp to 0
                    sourceStock.LastUpdatedAt = DateTime.UtcNow;
                    _stockRepository.Update(sourceStock);
                }
                item.Status = TransferOrderItemStatus.Pending;
            }
        }

        to.Status = TransferOrderStatus.Cancelled;
        _repository.Update(to);

        return Result.Success();
    }
}
