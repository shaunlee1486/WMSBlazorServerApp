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

namespace WMS.Application.Features.StockAdjustment.Commands;

public record ApproveStockAdjustmentCommand(Guid Id) : IRequest<Result>;

public class ApproveStockAdjustmentCommandHandler : IRequestHandler<ApproveStockAdjustmentCommand, Result>
{
    private readonly IStockAdjustmentRepository _repository;
    private readonly IStockRepository _stockRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdGenerator _idGenerator;

    public ApproveStockAdjustmentCommandHandler(
        IStockAdjustmentRepository repository,
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

    public async Task<Result> Handle(ApproveStockAdjustmentCommand request, CancellationToken cancellationToken)
    {
        // 1. Role enforcement: WarehouseManager or Admin
        var isAuthorized = _currentUserService.Roles.Any(role => 
            role.Equals("WarehouseManager", StringComparison.OrdinalIgnoreCase) || 
            role.Equals("Admin", StringComparison.OrdinalIgnoreCase));

        if (!isAuthorized)
        {
            return Result.Failure("Unauthorized. Only Warehouse Managers or Administrators can approve stock adjustments.");
        }

        // 2. Retrieve the adjustment with details
        var sa = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (sa == null)
        {
            return Result.Failure($"Stock adjustment with ID '{request.Id}' was not found.");
        }

        if (sa.Status != AdjustmentStatus.PendingApproval)
        {
            return Result.Failure($"Only stock adjustments in PendingApproval status can be approved. Current status: {sa.Status}");
        }

        var userId = _currentUserService.UserId ?? Guid.Empty;

        // 3. Process items, modify stock, and write movement logs
        foreach (var item in sa.Items)
        {
            if (item.Difference == 0)
            {
                continue; // No stock change
            }

            var stockRecords = await _stockRepository.FindAsync(
                s => s.ProductId == item.ProductId && s.LocationId == item.LocationId, 
                cancellationToken);

            var stock = stockRecords.FirstOrDefault();

            if (stock != null)
            {
                var newQty = stock.Quantity + item.Difference;
                if (newQty < 0)
                {
                    return Result.Failure($"Approval failed. Applying adjustment for product code '{item.Product.Code}' at location '{item.Location.Barcode}' would result in negative stock quantity ({newQty}).");
                }

                stock.Quantity = newQty;
                stock.LastUpdatedAt = DateTime.UtcNow;
                _stockRepository.Update(stock);
            }
            else
            {
                // Stock record doesn't exist, create one
                if (item.Difference < 0)
                {
                    return Result.Failure($"Approval failed. Cannot decrease stock for product code '{item.Product.Code}' at location '{item.Location.Barcode}' because no stock record exists.");
                }

                stock = new WMS.Domain.Entities.Inventory.Stock
                {
                    Id = _idGenerator.Generate(),
                    ProductId = item.ProductId,
                    LocationId = item.LocationId,
                    Quantity = item.Difference,
                    ReservedQuantity = 0,
                    LastUpdatedAt = DateTime.UtcNow
                };

                await _stockRepository.AddAsync(stock, cancellationToken);
            }

            // Create StockMovement record
            var movement = new StockMovement
            {
                Id = _idGenerator.Generate(),
                ProductId = item.ProductId,
                FromLocationId = item.Difference < 0 ? item.LocationId : null,
                ToLocationId = item.Difference > 0 ? item.LocationId : null,
                Quantity = Math.Abs(item.Difference),
                MovementType = MovementType.Adjustment,
                ReferenceNo = sa.AdjNumber,
                Note = $"Stock Adjustment Approval: {sa.Reason ?? "No reason provided"}",
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _movementRepository.AddAsync(movement, cancellationToken);
        }

        // 4. Update adjustment status
        sa.Status = AdjustmentStatus.Approved;
        sa.ApprovedBy = userId;
        _repository.Update(sa);

        return Result.Success();
    }
}
