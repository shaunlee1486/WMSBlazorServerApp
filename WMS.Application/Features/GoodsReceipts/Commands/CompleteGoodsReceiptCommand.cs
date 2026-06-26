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

namespace WMS.Application.Features.GoodsReceipts.Commands;

public record CompleteGoodsReceiptCommand(Guid Id) : IRequest<Result>;

public class CompleteGoodsReceiptCommandHandler : IRequestHandler<CompleteGoodsReceiptCommand, Result>
{
    private readonly IGoodsReceiptRepository _repository;
    private readonly IPurchaseOrderRepository _poRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly IIdGenerator _idGenerator;

    public CompleteGoodsReceiptCommandHandler(
        IGoodsReceiptRepository repository,
        IPurchaseOrderRepository poRepository,
        IStockRepository stockRepository,
        IStockMovementRepository movementRepository,
        IIdGenerator idGenerator)
    {
        _repository = repository;
        _poRepository = poRepository;
        _stockRepository = stockRepository;
        _movementRepository = movementRepository;
        _idGenerator = idGenerator;
    }

    public async Task<Result> Handle(CompleteGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve the Goods Receipt with items
        var gr = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (gr == null)
        {
            return Result.Failure($"Goods receipt with ID '{request.Id}' was not found.");
        }

        if (gr.Status != GoodsReceiptStatus.Draft)
        {
            return Result.Failure($"Only draft goods receipts can be completed. Current status: {gr.Status}");
        }

        // 2. Retrieve the associated Purchase Order
        var po = await _poRepository.GetByIdWithItemsAsync(gr.PurchaseOrderId, cancellationToken);
        if (po == null)
        {
            return Result.Failure($"Linked purchase order with ID '{gr.PurchaseOrderId}' was not found.");
        }

        // 3. Process each receipt item: modify stock level, write movement ledger, increment PO received qty
        foreach (var grItem in gr.Items)
        {
            if (grItem.ReceivedQty <= 0)
            {
                return Result.Failure($"Received quantity for product '{grItem.Product.Code}' must be greater than 0.");
            }

            // A. Update Stock
            var stockRecords = await _stockRepository.FindAsync(
                s => s.ProductId == grItem.ProductId && s.LocationId == grItem.LocationId, 
                cancellationToken);
            
            var stock = stockRecords.FirstOrDefault();
            if (stock != null)
            {
                stock.Quantity += grItem.ReceivedQty;
                stock.LastUpdatedAt = DateTime.UtcNow;
                _stockRepository.Update(stock);
            }
            else
            {
                stock = new WMS.Domain.Entities.Inventory.Stock
                {
                    Id = _idGenerator.Generate(),
                    ProductId = grItem.ProductId,
                    LocationId = grItem.LocationId,
                    Quantity = grItem.ReceivedQty,
                    ReservedQuantity = 0,
                    LastUpdatedAt = DateTime.UtcNow
                };
                await _stockRepository.AddAsync(stock, cancellationToken);
            }

            // B. Add StockMovement
            var movement = new StockMovement
            {
                Id = _idGenerator.Generate(),
                ProductId = grItem.ProductId,
                FromLocationId = null,
                ToLocationId = grItem.LocationId,
                Quantity = grItem.ReceivedQty,
                MovementType = MovementType.Receipt,
                ReferenceNo = gr.GRNumber,
                Note = $"Goods Receipt: {gr.GRNumber} against PO {po.PONumber}",
                CreatedBy = gr.ReceivedBy,
                CreatedAt = DateTime.UtcNow
            };
            await _movementRepository.AddAsync(movement, cancellationToken);

            // C. Increment PO item received qty
            var poItem = po.Items.FirstOrDefault(poi => poi.ProductId == grItem.ProductId);
            if (poItem != null)
            {
                poItem.ReceivedQty += grItem.ReceivedQty;
            }
        }

        // 4. Recalculate Purchase Order status
        var allFullyReceived = po.Items.All(poi => poi.ReceivedQty >= poi.OrderedQty);
        var anyReceived = po.Items.Any(poi => poi.ReceivedQty > 0);

        if (allFullyReceived)
        {
            po.Status = PurchaseOrderStatus.FullyReceived;
        }
        else if (anyReceived)
        {
            po.Status = PurchaseOrderStatus.PartialReceived;
        }
        else
        {
            po.Status = PurchaseOrderStatus.Confirmed;
        }
        
        _poRepository.Update(po);

        // 5. Update Goods Receipt status
        gr.Status = GoodsReceiptStatus.Completed;
        _repository.Update(gr);

        return Result.Success();
    }
}
