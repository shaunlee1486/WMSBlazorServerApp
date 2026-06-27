using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.PickList.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.PickList.Queries;

public record GetPickListsQuery(
    string? SearchTerm,
    string? Status,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<PickListDto>>>;

public class GetPickListsQueryHandler : IRequestHandler<GetPickListsQuery, Result<PagedResult<PickListDto>>>
{
    private readonly IPickListRepository _repository;

    public GetPickListsQueryHandler(IPickListRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<PickListDto>>> Handle(GetPickListsQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await _repository.GetPickListsPagedAsync(
            request.SearchTerm,
            request.Status,
            request.SortColumn,
            request.SortOrder,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = pagedResult.Items.Select(pl => new PickListDto
        {
            Id = pl.Id,
            PickListNumber = pl.PickListNumber,
            SalesOrderId = pl.SalesOrderId,
            SONumber = pl.SalesOrder.SONumber,
            CustomerName = pl.SalesOrder.Customer.Name,
            AssignedTo = pl.AssignedTo,
            Status = pl.Status.ToString(),
            CreatedAt = pl.CreatedAt
        }).ToList();

        var result = new PagedResult<PickListDto>(dtos, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize);
        return Result.Success(result);
    }
}
