using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.SalesOrders.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.SalesOrders.Queries;

public record GetSalesOrdersQuery(
    string? SearchTerm,
    string? Status,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<SalesOrderDto>>>;

public class GetSalesOrdersQueryHandler : IRequestHandler<GetSalesOrdersQuery, Result<PagedResult<SalesOrderDto>>>
{
    private readonly ISalesOrderRepository _repository;

    public GetSalesOrdersQueryHandler(ISalesOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<SalesOrderDto>>> Handle(GetSalesOrdersQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await _repository.GetSalesOrdersPagedAsync(
            request.SearchTerm,
            request.Status,
            request.SortColumn,
            request.SortOrder,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = pagedResult.Items.Select(so => new SalesOrderDto
        {
            Id = so.Id,
            SONumber = so.SONumber,
            CustomerId = so.CustomerId,
            CustomerName = so.Customer.Name,
            Status = so.Status.ToString(),
            OrderDate = so.OrderDate,
            RequiredDate = so.RequiredDate,
            Note = so.Note,
            CreatedBy = so.CreatedBy,
            CreatedAt = so.CreatedAt
        }).ToList();

        var result = new PagedResult<SalesOrderDto>(dtos, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize);
        return Result.Success(result);
    }
}
