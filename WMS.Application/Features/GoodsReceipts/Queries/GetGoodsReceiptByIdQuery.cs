using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.GoodsReceipts.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.GoodsReceipts.Queries;

public record GetGoodsReceiptByIdQuery(Guid Id) : IRequest<Result<GoodsReceiptDto>>;

public class GetGoodsReceiptByIdQueryHandler : IRequestHandler<GetGoodsReceiptByIdQuery, Result<GoodsReceiptDto>>
{
    private readonly IGoodsReceiptRepository _repository;

    public GetGoodsReceiptByIdQueryHandler(IGoodsReceiptRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GoodsReceiptDto>> Handle(GetGoodsReceiptByIdQuery request, CancellationToken cancellationToken)
    {
        var gr = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (gr == null)
        {
            return Result.Failure<GoodsReceiptDto>($"Goods receipt with ID '{request.Id}' was not found.");
        }

        var dto = new GoodsReceiptDto
        {
            Id = gr.Id,
            GRNumber = gr.GRNumber,
            PurchaseOrderId = gr.PurchaseOrderId,
            PONumber = gr.PurchaseOrder.PONumber,
            SupplierName = gr.PurchaseOrder.Supplier.Name,
            ReceivedDate = gr.ReceivedDate,
            ReceivedBy = gr.ReceivedBy,
            Status = gr.Status.ToString(),
            Note = gr.Note,
            CreatedAt = gr.CreatedAt,
            Items = gr.Items.Select(gri => new GoodsReceiptItemDto
            {
                Id = gri.Id,
                ProductId = gri.ProductId,
                ProductCode = gri.Product.Code,
                ProductName = gri.Product.Name,
                LocationId = gri.LocationId,
                LocationBarcode = gri.Location.Barcode,
                ReceivedQty = gri.ReceivedQty,
                BatchNo = gri.BatchNo,
                ExpiryDate = gri.ExpiryDate
            }).ToList()
        };

        return Result.Success(dto);
    }
}
