using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.Stock.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.Stock.Queries;

public record GetStockOverviewQuery(
    string? SearchTerm,
    Guid? WarehouseId,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<StockOverviewDto>>>;

public class GetStockOverviewQueryHandler : IRequestHandler<GetStockOverviewQuery, Result<PagedResult<StockOverviewDto>>>
{
    private readonly IStockRepository _stockRepository;

    public GetStockOverviewQueryHandler(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<Result<PagedResult<StockOverviewDto>>> Handle(GetStockOverviewQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await _stockRepository.GetStockOverviewPagedAsync(
            request.SearchTerm,
            request.WarehouseId,
            request.SortColumn,
            request.SortOrder,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = pagedResult.Items.Select(s => new StockOverviewDto
        {
            Id = s.Id,
            ProductId = s.ProductId,
            ProductCode = s.Product.Code,
            ProductName = s.Product.Name,
            LocationId = s.LocationId,
            LocationBarcode = s.Location.Barcode,
            WarehouseName = s.Location.Zone.Warehouse.Name,
            Quantity = s.Quantity,
            ReservedQuantity = s.ReservedQuantity,
            AvailableQuantity = s.AvailableQuantity,
            LastUpdatedAt = s.LastUpdatedAt
        }).ToList();

        var result = new PagedResult<StockOverviewDto>(dtos, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize);
        return Result.Success(result);
    }
}
