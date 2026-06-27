using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.Dashboard.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.Dashboard.Queries;

public record GetInboundOutboundVolumeQuery : IRequest<Result<List<InboundOutboundVolumeDto>>>;

public class GetInboundOutboundVolumeQueryHandler : IRequestHandler<GetInboundOutboundVolumeQuery, Result<List<InboundOutboundVolumeDto>>>
{
    private readonly IStockMovementRepository _movementRepository;

    public GetInboundOutboundVolumeQueryHandler(IStockMovementRepository movementRepository)
    {
        _movementRepository = movementRepository;
    }

    public async Task<Result<List<InboundOutboundVolumeDto>>> Handle(GetInboundOutboundVolumeQuery request, CancellationToken cancellationToken)
    {
        var thirtyDaysAgo = DateTime.UtcNow.Date.AddDays(-30);
        var movements = await _movementRepository.FindAsync(m => m.CreatedAt >= thirtyDaysAgo, cancellationToken);

        // Group by local date to match daily volumes
        var dateRange = Enumerable.Range(0, 30)
            .Select(i => thirtyDaysAgo.AddDays(i).Date)
            .ToList();

        var dailyMovements = movements
            .GroupBy(m => m.CreatedAt.ToLocalTime().Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        var dtos = dateRange.Select(date =>
        {
            decimal inbound = 0;
            decimal outbound = 0;

            if (dailyMovements.TryGetValue(date, out var list))
            {
                inbound = list.Where(m => m.MovementType == MovementType.Receipt).Sum(m => m.Quantity);
                outbound = list.Where(m => m.MovementType == MovementType.Issue).Sum(m => m.Quantity);
            }

            return new InboundOutboundVolumeDto
            {
                Date = date,
                InboundQuantity = inbound,
                OutboundQuantity = outbound
            };
        }).ToList();

        return Result.Success(dtos);
    }
}
