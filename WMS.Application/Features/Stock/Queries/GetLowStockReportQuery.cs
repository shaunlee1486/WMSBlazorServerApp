using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.Stock.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.Stock.Queries;

public record GetLowStockReportQuery : IRequest<Result<IReadOnlyList<LowStockDto>>>;

public class GetLowStockReportQueryHandler : IRequestHandler<GetLowStockReportQuery, Result<IReadOnlyList<LowStockDto>>>
{
    private readonly IStockRepository _stockRepository;

    public GetLowStockReportQueryHandler(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<Result<IReadOnlyList<LowStockDto>>> Handle(GetLowStockReportQuery request, CancellationToken cancellationToken)
    {
        var lowStockItems = await _stockRepository.GetLowStockReportAsync(cancellationToken);

        // Group by product to aggregate stock across locations, or display each low-stock record?
        // Let's group by product to show total current stock vs min stock / reorder point.
        var dtos = lowStockItems
            .GroupBy(s => new { s.ProductId, s.Product.Code, s.Product.Name, CategoryName = s.Product.Category != null ? s.Product.Category.Name : string.Empty, s.Product.MinStock, s.Product.ReorderPoint })
            .Select(g => new LowStockDto
            {
                ProductId = g.Key.ProductId,
                ProductCode = g.Key.Code,
                ProductName = g.Key.Name,
                CategoryName = g.Key.CategoryName,
                CurrentStock = g.Sum(s => s.Quantity),
                MinStock = g.Key.MinStock,
                ReorderPoint = g.Key.ReorderPoint
            })
            .OrderBy(dto => dto.ProductCode)
            .ToList();

        return Result.Success<IReadOnlyList<LowStockDto>>(dtos);
    }
}
