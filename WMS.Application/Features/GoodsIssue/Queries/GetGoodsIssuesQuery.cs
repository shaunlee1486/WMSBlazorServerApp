using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.GoodsIssue.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.GoodsIssue.Queries;

public record GetGoodsIssuesQuery(
    string? SearchTerm,
    string? Status,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<GoodsIssueDto>>>;

public class GetGoodsIssuesQueryHandler : IRequestHandler<GetGoodsIssuesQuery, Result<PagedResult<GoodsIssueDto>>>
{
    private readonly IGoodsIssueRepository _repository;

    public GetGoodsIssuesQueryHandler(IGoodsIssueRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<GoodsIssueDto>>> Handle(GetGoodsIssuesQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await _repository.GetGoodsIssuesPagedAsync(
            request.SearchTerm,
            request.Status,
            request.SortColumn,
            request.SortOrder,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = pagedResult.Items.Select(gi => new GoodsIssueDto
        {
            Id = gi.Id,
            GINumber = gi.GINumber,
            SalesOrderId = gi.SalesOrderId,
            SONumber = gi.SalesOrder.SONumber,
            CustomerName = gi.SalesOrder.Customer.Name,
            IssuedDate = gi.IssuedDate,
            IssuedBy = gi.IssuedBy,
            Status = gi.Status.ToString(),
            Note = gi.Note,
            CreatedAt = gi.CreatedAt
        }).ToList();

        var result = new PagedResult<GoodsIssueDto>(dtos, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize);
        return Result.Success(result);
    }
}
