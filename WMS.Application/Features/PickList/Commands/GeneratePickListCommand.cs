using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Domain.Entities.Outbound;
using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.PickList.Commands;

public record GeneratePickListCommand(Guid SalesOrderId) : IRequest<Result<Guid>>;

public class GeneratePickListCommandHandler : IRequestHandler<GeneratePickListCommand, Result<Guid>>
{
    private readonly IPickListRepository _repository;
    private readonly ISalesOrderRepository _soRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly IIdGenerator _idGenerator;

    public GeneratePickListCommandHandler(
        IPickListRepository repository,
        ISalesOrderRepository soRepository,
        IStockRepository stockRepository,
        IStockMovementRepository movementRepository,
        IIdGenerator idGenerator)
    {
        _repository = repository;
        _soRepository = soRepository;
        _stockRepository = stockRepository;
        _movementRepository = movementRepository;
        _idGenerator = idGenerator;
    }

    public async Task<Result<Guid>> Handle(GeneratePickListCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify Sales Order
        var so = await _soRepository.GetByIdWithItemsAsync(request.SalesOrderId, cancellationToken);
        if (so == null)
        {
            return Result.Failure<Guid>($"Sales order with ID '{request.SalesOrderId}' was not found.");
        }

        if (so.Status != SalesOrderStatus.Confirmed)
        {
            return Result.Failure<Guid>($"Pick lists can only be generated for confirmed sales orders. Current status: {so.Status}");
        }

        // 2. Check if active pick list already exists
        var existingPLs = await _repository.FindAsync(
            pl => pl.SalesOrderId == so.Id && pl.Status != PickListStatus.Cancelled, 
            cancellationToken);
        
        if (existingPLs.Any())
        {
            return Result.Failure<Guid>($"An active pick list already exists for Sales Order {so.SONumber}.");
        }

        // 3. Generate Pick List
        var pickListNumber = await _repository.GetNextPickListNumberAsync(cancellationToken);
        var pl = new WMS.Domain.Entities.Outbound.PickList
        {
            Id = _idGenerator.Generate(),
            PickListNumber = pickListNumber,
            SalesOrderId = so.Id,
            Status = PickListStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // 4. Suggest FIFO locations for each item
        foreach (var item in so.Items)
        {
            var stocks = await _stockRepository.FindAsync(s => s.ProductId == item.ProductId && s.Quantity > 0, cancellationToken);
            
            // Get receipt movements to sort by FIFO (oldest receipt first)
            var movements = await _movementRepository.FindAsync(
                sm => sm.ProductId == item.ProductId && sm.MovementType == MovementType.Receipt && sm.ToLocationId != null,
                cancellationToken);

            var oldestReceipts = movements
                .GroupBy(sm => sm.ToLocationId!.Value)
                .Select(g => new { LocationId = g.Key, OldestCreatedAt = g.Min(sm => sm.CreatedAt) })
                .ToList();

            var sortedStocks = stocks
                .Select(s => new {
                    Stock = s,
                    OldestReceipt = oldestReceipts.FirstOrDefault(r => r.LocationId == s.LocationId)?.OldestCreatedAt ?? DateTime.MaxValue
                })
                .OrderBy(x => x.OldestReceipt)
                .Select(x => x.Stock)
                .ToList();

            var remainingToPick = item.OrderedQty;
            foreach (var stock in sortedStocks)
            {
                var qtyToPick = Math.Min(stock.Quantity, remainingToPick);
                if (qtyToPick <= 0) continue;

                var plItem = new PickListItem
                {
                    Id = _idGenerator.Generate(),
                    PickListId = pl.Id,
                    ProductId = item.ProductId,
                    LocationId = stock.LocationId,
                    RequiredQty = qtyToPick,
                    PickedQty = 0,
                    Status = "Pending"
                };

                pl.Items.Add(plItem);
                remainingToPick -= qtyToPick;

                if (remainingToPick <= 0) break;
            }

            // If we still have quantities to pick but ran out of stock records,
            // we will allocate the remainder to the first stock location (or a default if empty)
            if (remainingToPick > 0)
            {
                var defaultLocationId = sortedStocks.FirstOrDefault()?.LocationId 
                    ?? stocks.FirstOrDefault()?.LocationId 
                    ?? Guid.Empty;

                if (defaultLocationId != Guid.Empty)
                {
                    var existingPlItem = pl.Items.FirstOrDefault(i => i.LocationId == defaultLocationId && i.ProductId == item.ProductId);
                    if (existingPlItem != null)
                    {
                        existingPlItem.RequiredQty += remainingToPick;
                    }
                    else
                    {
                        pl.Items.Add(new PickListItem
                        {
                            Id = _idGenerator.Generate(),
                            PickListId = pl.Id,
                            ProductId = item.ProductId,
                            LocationId = defaultLocationId,
                            RequiredQty = remainingToPick,
                            PickedQty = 0,
                            Status = "Pending"
                        });
                    }
                }
            }
        }

        await _repository.AddAsync(pl, cancellationToken);
        return Result.Success(pl.Id);
    }
}
