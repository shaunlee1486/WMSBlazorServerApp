using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.Dashboard.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.Dashboard.Queries;

public record GetStockValueByCategoryQuery : IRequest<Result<List<StockValueByCategoryDto>>>;

public class GetStockValueByCategoryQueryHandler : IRequestHandler<GetStockValueByCategoryQuery, Result<List<StockValueByCategoryDto>>>
{
    private readonly IStockRepository _stockRepository;

    public GetStockValueByCategoryQueryHandler(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<Result<List<StockValueByCategoryDto>>> Handle(GetStockValueByCategoryQuery request, CancellationToken cancellationToken)
    {
        var stats = await _stockRepository.GetStockCategoryStatsAsync(cancellationToken);
        
        var dtos = stats.Select(s => new StockValueByCategoryDto
        {
            CategoryName = s.CategoryName,
            TotalQuantity = s.TotalQuantity,
            TotalValue = s.TotalValue
        }).ToList();

        return Result.Success(dtos);
    }
}
