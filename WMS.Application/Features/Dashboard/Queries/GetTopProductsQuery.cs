using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.Dashboard.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.Dashboard.Queries;

public record GetTopProductsQuery(int Count = 10) : IRequest<Result<List<TopProductDto>>>;

public class GetTopProductsQueryHandler : IRequestHandler<GetTopProductsQuery, Result<List<TopProductDto>>>
{
    private readonly IStockMovementRepository _movementRepository;

    public GetTopProductsQueryHandler(IStockMovementRepository movementRepository)
    {
        _movementRepository = movementRepository;
    }

    public async Task<Result<List<TopProductDto>>> Handle(GetTopProductsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _movementRepository.GetTopProductsStatsAsync(request.Count, cancellationToken);
        
        var dtos = stats.Select(s => new TopProductDto
        {
            ProductCode = s.ProductCode,
            ProductName = s.ProductName,
            TotalQuantity = s.TotalQuantity,
            MovementCount = s.MovementCount
        }).ToList();

        return Result.Success(dtos);
    }
}
