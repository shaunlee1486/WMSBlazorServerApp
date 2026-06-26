using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.GoodsReceipts.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.GoodsReceipts.Queries;

public record GetGoodsReceiptsQuery(
    string? SearchTerm,
    string? Status,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<GoodsReceiptDto>>>;

public class GetGoodsReceiptsQueryHandler : IRequestHandler<GetGoodsReceiptsQuery, Result<PagedResult<GoodsReceiptDto>>>
{
    private readonly IGoodsReceiptRepository _repository;

    public GetGoodsReceiptsQueryHandler(IGoodsReceiptRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<GoodsReceiptDto>>> Handle(GetGoodsReceiptsQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await _repository.GetGoodsReceiptsPagedAsync(
            request.SearchTerm,
            request.Status,
            request.SortColumn,
            request.SortOrder,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = pagedResult.Items.Select(gr => new GoodsReceiptDto
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
            CreatedAt = gr.CreatedAt
        }).ToList();

        var result = new PagedResult<GoodsReceiptDto>(dtos, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize);
        return Result.Success(result);
    }
}
