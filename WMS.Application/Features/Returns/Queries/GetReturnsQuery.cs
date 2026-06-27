using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.Returns.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.Returns.Queries;

public record GetReturnsQuery(
    string? SearchTerm,
    string? Status,
    string? ReturnType,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IRequest<PagedResult<ReturnDto>>;

public class GetReturnsQueryHandler : IRequestHandler<GetReturnsQuery, PagedResult<ReturnDto>>
{
    private readonly IReturnRepository _repository;

    public GetReturnsQueryHandler(IReturnRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ReturnDto>> Handle(GetReturnsQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await _repository.GetReturnsPagedAsync(
            request.SearchTerm,
            request.Status,
            request.ReturnType,
            request.SortColumn,
            request.SortOrder,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = pagedResult.Items.Select(r => new ReturnDto
        {
            Id = r.Id,
            ReturnNumber = r.ReturnNumber,
            ReturnType = r.ReturnType.ToString(),
            ReferenceNo = r.ReferenceNo,
            Status = r.Status.ToString(),
            Note = r.Note,
            CreatedBy = r.CreatedBy,
            CreatedAt = r.CreatedAt
        }).ToList();

        return new PagedResult<ReturnDto>(dtos, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize);
    }
}
