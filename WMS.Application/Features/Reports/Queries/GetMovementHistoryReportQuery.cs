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

public record GetMovementHistoryReportQuery(
    Guid? ProductId,
    Guid? LocationId,
    string? MovementType,
    DateTime? StartDate,
    DateTime? EndDate,
    string? SearchTerm) : IRequest<Result<List<MovementReportDto>>>;

public class GetMovementHistoryReportQueryHandler : IRequestHandler<GetMovementHistoryReportQuery, Result<List<MovementReportDto>>>
{
    private readonly IStockMovementRepository _movementRepository;

    public GetMovementHistoryReportQueryHandler(IStockMovementRepository movementRepository)
    {
        _movementRepository = movementRepository;
    }

    public async Task<Result<List<MovementReportDto>>> Handle(GetMovementHistoryReportQuery request, CancellationToken cancellationToken)
    {
        var items = await _movementRepository.GetMovementHistoryReportAsync(
            request.ProductId,
            request.LocationId,
            request.MovementType,
            request.StartDate,
            request.EndDate,
            request.SearchTerm,
            cancellationToken);

        var dtos = items.Select(m => new MovementReportDto
        {
            Id = m.Id,
            CreatedAt = m.CreatedAt,
            ProductCode = m.Product.Code,
            ProductName = m.Product.Name,
            FromLocationBarcode = m.FromLocation?.Barcode ?? "-",
            ToLocationBarcode = m.ToLocation?.Barcode ?? "-",
            Quantity = m.Quantity,
            MovementType = m.MovementType.ToString(),
            ReferenceNo = m.ReferenceNo,
            Note = m.Note
        }).ToList();

        return Result.Success(dtos);
    }
}
