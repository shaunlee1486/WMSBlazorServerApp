using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.Warehouses.DTOs;
using WMS.Domain.Entities.MasterData;
using WMS.Domain.Interfaces;
using WMS.SharedKernel;

namespace WMS.Application.Features.Warehouses.Queries;

public record GetWarehousesQuery(
    string? SearchTerm,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<WarehouseDto>>>;

public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, Result<PagedResult<WarehouseDto>>>
{
    private readonly IRepository<Warehouse> _repository;

    public GetWarehousesQueryHandler(IRepository<Warehouse> repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<WarehouseDto>>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Warehouse, bool>>? predicate = null;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            predicate = w => w.Code.ToLower().Contains(search) 
                          || w.Name.ToLower().Contains(search) 
                          || (w.Address != null && w.Address.ToLower().Contains(search));
        }

        var sortColumn = request.SortColumn;
        var sortOrder = request.SortOrder;
        if (string.IsNullOrWhiteSpace(sortColumn))
        {
            sortColumn = nameof(Warehouse.Code);
            sortOrder = "asc";
        }

        var pagedResult = await _repository.GetPagedAsync(
            predicate,
            sortColumn,
            sortOrder,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = pagedResult.Items.Select(w => new WarehouseDto
        {
            Id = w.Id,
            Code = w.Code,
            Name = w.Name,
            Address = w.Address,
            IsActive = w.IsActive,
            CreatedAt = w.CreatedAt
        }).ToList();

        var result = new PagedResult<WarehouseDto>(dtos, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize);

        return Result.Success(result);
    }
}
