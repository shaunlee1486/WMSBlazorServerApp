using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.Reports.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.Reports.Queries;

public record GetStockSnapshotQuery(Guid? WarehouseId, Guid? CategoryId, string? SearchTerm) : IRequest<Result<List<StockSnapshotDto>>>;

public class GetStockSnapshotQueryHandler : IRequestHandler<GetStockSnapshotQuery, Result<List<StockSnapshotDto>>>
{
    private readonly IStockRepository _stockRepository;

    public GetStockSnapshotQueryHandler(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<Result<List<StockSnapshotDto>>> Handle(GetStockSnapshotQuery request, CancellationToken cancellationToken)
    {
        var items = await _stockRepository.GetStockSnapshotReportAsync(request.WarehouseId, request.CategoryId, request.SearchTerm, cancellationToken);
        
        var dtos = items.Select(s => new StockSnapshotDto
        {
            ProductCode = s.Product.Code,
            ProductName = s.Product.Name,
            CategoryName = s.Product.Category.Name,
            WarehouseName = s.Location.Zone.Warehouse.Name,
            LocationBarcode = s.Location.Barcode ?? string.Empty,
            Quantity = s.Quantity,
            ReservedQuantity = s.ReservedQuantity
        }).ToList();

        return Result.Success(dtos);
    }
}
