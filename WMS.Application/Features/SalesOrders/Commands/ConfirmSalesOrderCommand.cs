using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.SalesOrders.Commands;

public record ConfirmSalesOrderCommand(Guid Id) : IRequest<Result>;

public class ConfirmSalesOrderCommandHandler : IRequestHandler<ConfirmSalesOrderCommand, Result>
{
    private readonly ISalesOrderRepository _repository;
    private readonly IStockRepository _stockRepository;
    private readonly IStockMovementRepository _movementRepository;

    public ConfirmSalesOrderCommandHandler(
        ISalesOrderRepository repository,
        IStockRepository stockRepository,
        IStockMovementRepository movementRepository)
    {
        _repository = repository;
        _stockRepository = stockRepository;
        _movementRepository = movementRepository;
    }

    public async Task<Result> Handle(ConfirmSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var so = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (so == null)
        {
            return Result.Failure($"Sales order with ID '{request.Id}' was not found.");
        }

        if (so.Status != SalesOrderStatus.Draft)
        {
            return Result.Failure($"Only draft sales orders can be confirmed. Current status: {so.Status}");
        }

        // 1. Verify stock availability for all items first
        foreach (var item in so.Items)
        {
            var stocks = await _stockRepository.FindAsync(s => s.ProductId == item.ProductId, cancellationToken);
            var totalAvailable = stocks.Sum(s => s.Quantity - s.ReservedQuantity);

            if (totalAvailable < item.OrderedQty)
            {
                return Result.Failure($"Insufficient stock for product '{item.Product.Code}'. Ordered: {item.OrderedQty}, Available: {totalAvailable}");
            }
        }

        // 2. Reserve stock in locations using FIFO
        foreach (var item in so.Items)
        {
            var stocks = await _stockRepository.FindAsync(s => s.ProductId == item.ProductId, cancellationToken);
            
            // Get all receipt movements for this product to sort by FIFO (oldest receipt first)
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

            var remainingToReserve = item.OrderedQty;
            foreach (var stock in sortedStocks)
            {
                var availableInLocation = stock.Quantity - stock.ReservedQuantity;
                if (availableInLocation <= 0) continue;

                var qtyToReserve = Math.Min(availableInLocation, remainingToReserve);
                stock.ReservedQuantity += qtyToReserve;
                remainingToReserve -= qtyToReserve;

                _stockRepository.Update(stock);

                if (remainingToReserve <= 0) break;
            }

            if (remainingToReserve > 0)
            {
                return Result.Failure($"Concurrency error: unable to reserve all required quantity for product '{item.Product.Code}'.");
            }
        }

        // 3. Update Sales Order status to Confirmed
        so.Status = SalesOrderStatus.Confirmed;
        _repository.Update(so);

        return Result.Success();
    }
}
