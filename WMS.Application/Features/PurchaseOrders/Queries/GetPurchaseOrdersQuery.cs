using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.PurchaseOrders.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.PurchaseOrders.Queries;

public record GetPurchaseOrdersQuery(
    string? SearchTerm,
    string? Status,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<PurchaseOrderDto>>>;

public class GetPurchaseOrdersQueryHandler : IRequestHandler<GetPurchaseOrdersQuery, Result<PagedResult<PurchaseOrderDto>>>
{
    private readonly IPurchaseOrderRepository _repository;

    public GetPurchaseOrdersQueryHandler(IPurchaseOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<PurchaseOrderDto>>> Handle(GetPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await _repository.GetPurchaseOrdersPagedAsync(
            request.SearchTerm,
            request.Status,
            request.SortColumn,
            request.SortOrder,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = pagedResult.Items.Select(po => new PurchaseOrderDto
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
            CreatedAt = po.CreatedAt
        }).ToList();

        var result = new PagedResult<PurchaseOrderDto>(dtos, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize);
        return Result.Success(result);
    }
}
