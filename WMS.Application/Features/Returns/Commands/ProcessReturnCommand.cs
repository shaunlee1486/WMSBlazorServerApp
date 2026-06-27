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

namespace WMS.Application.Features.Returns.Commands;

public record ProcessReturnCommand(Guid Id) : IRequest<Result>;

public class ProcessReturnCommandHandler : IRequestHandler<ProcessReturnCommand, Result>
{
    private readonly IReturnRepository _repository;
    private readonly IStockRepository _stockRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdGenerator _idGenerator;

    public ProcessReturnCommandHandler(
        IReturnRepository repository,
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

    public async Task<Result> Handle(ProcessReturnCommand request, CancellationToken cancellationToken)
    {
        var ret = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (ret == null)
        {
            return Result.Failure($"Return with ID '{request.Id}' was not found.");
        }

        if (ret.Status != ReturnStatus.Pending && ret.Status != ReturnStatus.Processing)
        {
            return Result.Failure($"Only pending or processing returns can be finalized. Current status: {ret.Status}");
        }

        var userId = _currentUserService.UserId ?? Guid.Empty;

        // Customer Return validation: ensure no items are still pending inspection
        if (ret.ReturnType == ReturnType.Customer)
        {
            var pendingInspections = ret.Items.Any(i => i.InspectionStatus == InspectionStatus.Pending);
            if (pendingInspections)
            {
                return Result.Failure("Cannot finalize customer return while there are items pending inspection.");
            }
        }

        foreach (var item in ret.Items)
        {
            if (ret.ReturnType == ReturnType.Supplier)
            {
                // 1. Supplier Return: Decrement stock from the location
                var stocks = await _stockRepository.FindAsync(s => s.ProductId == item.ProductId && s.LocationId == item.LocationId, cancellationToken);
                var stock = stocks.FirstOrDefault();
                if (stock == null)
                {
                    return Result.Failure($"Stock record not found for product '{item.Product.Code}' at location '{item.LocationId}' to return to supplier.");
                }

                if (stock.Quantity - stock.ReservedQuantity < item.Quantity)
                {
                    return Result.Failure($"Insufficient available stock for product '{item.Product.Code}' at location '{item.LocationId}'. Available: {stock.Quantity - stock.ReservedQuantity}, Required to return: {item.Quantity}.");
                }

                stock.Quantity -= item.Quantity;
                stock.LastUpdatedAt = DateTime.UtcNow;
                _stockRepository.Update(stock);

                // Create Stock Movement
                var movement = new StockMovement
                {
                    Id = _idGenerator.Generate(),
                    ProductId = item.ProductId,
                    FromLocationId = item.LocationId,
                    ToLocationId = null,
                    Quantity = item.Quantity,
                    MovementType = MovementType.Return,
                    ReferenceNo = ret.ReturnNumber,
                    Note = $"Supplier return. Ref: {ret.ReferenceNo}. Item Note: {item.Note}",
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };
                await _movementRepository.AddAsync(movement, cancellationToken);
            }
            else if (ret.ReturnType == ReturnType.Customer)
            {
                // 2. Customer Return: Increment stock only for Accepted items
                if (item.InspectionStatus == InspectionStatus.Accepted)
                {
                    var stocks = await _stockRepository.FindAsync(s => s.ProductId == item.ProductId && s.LocationId == item.LocationId, cancellationToken);
                    var stock = stocks.FirstOrDefault();
                    if (stock == null)
                    {
                        stock = new WMS.Domain.Entities.Inventory.Stock
                        {
                            Id = _idGenerator.Generate(),
                            ProductId = item.ProductId,
                            LocationId = item.LocationId,
                            Quantity = item.Quantity,
                            ReservedQuantity = 0,
                            LastUpdatedAt = DateTime.UtcNow
                        };
                        await _stockRepository.AddAsync(stock, cancellationToken);
                    }
                    else
                    {
                        stock.Quantity += item.Quantity;
                        stock.LastUpdatedAt = DateTime.UtcNow;
                        _stockRepository.Update(stock);
                    }

                    // Create Stock Movement
                    var movement = new StockMovement
                    {
                        Id = _idGenerator.Generate(),
                        ProductId = item.ProductId,
                        FromLocationId = null,
                        ToLocationId = item.LocationId,
                        Quantity = item.Quantity,
                        MovementType = MovementType.Return,
                        ReferenceNo = ret.ReturnNumber,
                        Note = $"Customer return (Accepted). Ref: {ret.ReferenceNo}. Item Note: {item.Note}",
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _movementRepository.AddAsync(movement, cancellationToken);
                }
                else
                {
                    // Item was Rejected - we do not update inventory, but we record it in the return history
                    // Optionally, we could put it in a virtual location or write a note, but the default behavior is sufficient.
                }
            }
        }

        ret.Status = ReturnStatus.Completed;
        _repository.Update(ret);

        return Result.Success();
    }
}
