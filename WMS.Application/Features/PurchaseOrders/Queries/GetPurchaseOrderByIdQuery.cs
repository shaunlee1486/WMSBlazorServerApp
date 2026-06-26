using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.PurchaseOrders.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.PurchaseOrders.Queries;

public record GetPurchaseOrderByIdQuery(Guid Id) : IRequest<Result<PurchaseOrderDto>>;

public class GetPurchaseOrderByIdQueryHandler : IRequestHandler<GetPurchaseOrderByIdQuery, Result<PurchaseOrderDto>>
{
    private readonly IPurchaseOrderRepository _repository;

    public GetPurchaseOrderByIdQueryHandler(IPurchaseOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PurchaseOrderDto>> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var po = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (po == null)
        {
            return Result.Failure<PurchaseOrderDto>($"Purchase order with ID '{request.Id}' was not found.");
        }

        var dto = new PurchaseOrderDto
        {
            Id = po.Id,
            PONumber = po.PONumber,
            SupplierId = po.SupplierId,
            SupplierName = po.Supplier.Name,
            Status = po.Status.ToString(),
            OrderDate = po.OrderDate,
            ExpectedDate = po.ExpectedDate,
            Note = po.Note,
            CreatedBy = po.CreatedBy,
            CreatedAt = po.CreatedAt,
            Items = po.Items.Select(poi => new PurchaseOrderItemDto
            {
                Id = poi.Id,
                ProductId = poi.ProductId,
                ProductCode = poi.Product.Code,
                ProductName = poi.Product.Name,
                OrderedQty = poi.OrderedQty,
                ReceivedQty = poi.ReceivedQty,
                UnitPrice = poi.UnitPrice
            }).ToList()
        };

        return Result.Success(dto);
    }
}
