using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.StockAdjustment.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.StockAdjustment.Queries;

public record GetStockAdjustmentsQuery(
    string? SearchTerm,
    Guid? WarehouseId,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<StockAdjustmentDto>>>;

public class GetStockAdjustmentsQueryHandler : IRequestHandler<GetStockAdjustmentsQuery, Result<PagedResult<StockAdjustmentDto>>>
{
    private readonly IStockAdjustmentRepository _repository;

    public GetStockAdjustmentsQueryHandler(IStockAdjustmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<StockAdjustmentDto>>> Handle(GetStockAdjustmentsQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await _repository.GetStockAdjustmentsPagedAsync(
            request.SearchTerm,
            request.WarehouseId,
            request.SortColumn,
            request.SortOrder,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = pagedResult.Items.Select(sa => new StockAdjustmentDto
        {
            Id = sa.Id,
            AdjNumber = sa.AdjNumber,
            WarehouseId = sa.WarehouseId,
            WarehouseName = sa.Warehouse.Name,
            AdjustmentDate = sa.AdjustmentDate,
            Reason = sa.Reason,
            ApprovedBy = sa.ApprovedBy,
            Status = sa.Status.ToString(),
            CreatedBy = sa.CreatedBy,
            CreatedAt = sa.CreatedAt
        }).ToList();

        var result = new PagedResult<StockAdjustmentDto>(dtos, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize);
        return Result.Success(result);
    }
}
