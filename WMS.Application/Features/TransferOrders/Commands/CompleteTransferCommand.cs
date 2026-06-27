using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Entities.Inventory;
using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.TransferOrders.Commands;

public record CompleteTransferCommand(Guid Id) : IRequest<Result>;

public class CompleteTransferCommandHandler : IRequestHandler<CompleteTransferCommand, Result>
{
    private readonly ITransferOrderRepository _repository;
    private readonly IStockRepository _stockRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdGenerator _idGenerator;

    public CompleteTransferCommandHandler(
        ITransferOrderRepository repository,
        IStockRepository stockRepository,
        IStockMovementRepository movementRepository,
        ICurrentUserService currentUserService,
        IIdGenerator idGenerator)
    {
        _repository = repository;
        _stockRepository = stockRepository;
        _movementRepository = movementRepository;
        _currentUserService = currentUserService;
        _idGenerator = idGenerator;
    }

    public async Task<Result> Handle(CompleteTransferCommand request, CancellationToken cancellationToken)
    {
        var to = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (to == null)
        {
            return Result.Failure($"Transfer order with ID '{request.Id}' was not found.");
        }

        if (to.Status != TransferOrderStatus.InTransit)
        {
            return Result.Failure($"Only in-transit transfer orders can be completed. Current status: {to.Status}");
        }

        var userId = _currentUserService.UserId ?? Guid.Empty;

        foreach (var item in to.Items)
        {
            // 1. Process Source Stock (decrement physical and reserved stock)
            var sourceStocks = await _stockRepository.FindAsync(s => s.ProductId == item.ProductId && s.LocationId == item.FromLocationId, cancellationToken);
            var sourceStock = sourceStocks.FirstOrDefault();
            if (sourceStock == null)
            {
                return Result.Failure($"Source stock record not found for product '{item.Product.Code}' at location '{item.FromLocationId}'.");
            }

            sourceStock.Quantity -= item.Qty;
            sourceStock.ReservedQuantity -= item.Qty;
            if (sourceStock.ReservedQuantity < 0) sourceStock.ReservedQuantity = 0; // clamp to 0
            sourceStock.LastUpdatedAt = DateTime.UtcNow;
            _stockRepository.Update(sourceStock);

            // 2. Process Destination Stock (increment quantity)
            var destStocks = await _stockRepository.FindAsync(s => s.ProductId == item.ProductId && s.LocationId == item.ToLocationId, cancellationToken);
            var destStock = destStocks.FirstOrDefault();
            if (destStock == null)
            {
                destStock = new WMS.Domain.Entities.Inventory.Stock
                {
                    Id = _idGenerator.Generate(),
                    ProductId = item.ProductId,
                    LocationId = item.ToLocationId,
                    Quantity = item.Qty,
                    ReservedQuantity = 0,
                    LastUpdatedAt = DateTime.UtcNow
                };
                await _stockRepository.AddAsync(destStock, cancellationToken);
            }
            else
            {
                destStock.Quantity += item.Qty;
                destStock.LastUpdatedAt = DateTime.UtcNow;
                _stockRepository.Update(destStock);
            }

            // 3. Register single Transfer StockMovement
            var movement = new StockMovement
            {
                Id = _idGenerator.Generate(),
                ProductId = item.ProductId,
                FromLocationId = item.FromLocationId,
                ToLocationId = item.ToLocationId,
                Quantity = item.Qty,
                MovementType = MovementType.Transfer,
                ReferenceNo = to.TONumber,
                Note = $"Inter-warehouse transfer from {to.FromWarehouse.Name} to {to.ToWarehouse.Name}",
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _movementRepository.AddAsync(movement, cancellationToken);

            item.Status = TransferOrderItemStatus.Received;
        }

        to.Status = TransferOrderStatus.Completed;
        _repository.Update(to);

        return Result.Success();
    }
}
