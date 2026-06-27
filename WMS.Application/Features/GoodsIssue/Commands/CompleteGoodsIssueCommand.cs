using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Domain.Entities.Inventory;
using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.GoodsIssue.Commands;

public record CompleteGoodsIssueItemInput(Guid ItemId, string? BatchNo);

public record CompleteGoodsIssueCommand(Guid Id, string? Note, List<CompleteGoodsIssueItemInput> Items) : IRequest<Result>;

public class CompleteGoodsIssueCommandHandler : IRequestHandler<CompleteGoodsIssueCommand, Result>
{
    private readonly IGoodsIssueRepository _repository;
    private readonly ISalesOrderRepository _soRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly IIdGenerator _idGenerator;

    public CompleteGoodsIssueCommandHandler(
        IGoodsIssueRepository repository,
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

    public async Task<Result> Handle(CompleteGoodsIssueCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch Goods Issue
        var gi = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (gi == null)
        {
            return Result.Failure($"Goods issue with ID '{request.Id}' was not found.");
        }

        if (gi.Status != GoodsIssueStatus.Draft)
        {
            return Result.Failure($"Only draft goods issues can be completed. Current status: {gi.Status}");
        }

        // 2. Fetch associated Sales Order
        var so = await _soRepository.GetByIdWithItemsAsync(gi.SalesOrderId, cancellationToken);
        if (so == null)
        {
            return Result.Failure($"Associated sales order with ID '{gi.SalesOrderId}' was not found.");
        }

        // 3. Process each Goods Issue item
        foreach (var giItem in gi.Items)
        {
            var inputItem = request.Items?.FirstOrDefault(i => i.ItemId == giItem.Id);
            if (inputItem != null)
            {
                giItem.BatchNo = inputItem.BatchNo;
            }

            if (giItem.IssuedQty <= 0)
            {
                return Result.Failure($"Issued quantity for product '{giItem.Product.Code}' must be greater than 0.");
            }

            // A. Update Stock
            var stockRecords = await _stockRepository.FindAsync(
                s => s.ProductId == giItem.ProductId && s.LocationId == giItem.LocationId,
                cancellationToken);
            
            var stock = stockRecords.FirstOrDefault();
            if (stock == null)
            {
                return Result.Failure($"Stock record not found for product '{giItem.Product.Code}' at the specified location.");
            }

            if (stock.Quantity < giItem.IssuedQty)
            {
                return Result.Failure($"Insufficient physical stock for product '{giItem.Product.Code}' at the location. Available: {stock.Quantity}, Requested: {giItem.IssuedQty}");
            }

            stock.Quantity -= giItem.IssuedQty;
            stock.ReservedQuantity = Math.Max(0, stock.ReservedQuantity - giItem.IssuedQty);
            stock.LastUpdatedAt = DateTime.UtcNow;

            _stockRepository.Update(stock);

            // B. Add StockMovement
            var movement = new StockMovement
            {
                Id = _idGenerator.Generate(),
                ProductId = giItem.ProductId,
                FromLocationId = giItem.LocationId,
                ToLocationId = null,
                Quantity = giItem.IssuedQty,
                MovementType = MovementType.Issue,
                ReferenceNo = gi.GINumber,
                Note = $"Goods Issue: {gi.GINumber} for SO {so.SONumber}",
                CreatedBy = gi.IssuedBy,
                CreatedAt = DateTime.UtcNow
            };
            await _movementRepository.AddAsync(movement, cancellationToken);
        }

        // 4. Update Sales Order status to Shipped
        so.Status = SalesOrderStatus.Shipped;
        _soRepository.Update(so);

        // 5. Update Goods Issue status to Completed
        gi.Status = GoodsIssueStatus.Completed;
        gi.IssuedDate = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            gi.Note = request.Note;
        }
        _repository.Update(gi);

        return Result.Success();
    }
}
