using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.Reports.DTOs;
using WMS.Domain.Entities.Identity;
using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.Reports.Queries;

public record GetAdjustmentReportQuery(Guid? WarehouseId, DateTime? StartDate, DateTime? EndDate) : IRequest<Result<List<AdjustmentReportDto>>>;

public class GetAdjustmentReportQueryHandler : IRequestHandler<GetAdjustmentReportQuery, Result<List<AdjustmentReportDto>>>
{
    private readonly IStockAdjustmentRepository _adjustmentRepository;
    private readonly IRepository<AppUser> _userRepository;

    public GetAdjustmentReportQueryHandler(IStockAdjustmentRepository adjustmentRepository, IRepository<AppUser> userRepository)
    {
        _adjustmentRepository = adjustmentRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<List<AdjustmentReportDto>>> Handle(GetAdjustmentReportQuery request, CancellationToken cancellationToken)
    {
        var items = await _adjustmentRepository.GetAdjustmentReportAsync(request.WarehouseId, request.StartDate, request.EndDate, cancellationToken);
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var userEmails = users.ToDictionary(u => u.Id, u => u.Email ?? string.Empty);

        var dtos = items.Select(sai =>
        {
            userEmails.TryGetValue(sai.StockAdjustment.CreatedBy, out var createdEmail);
            string approvedEmail = string.Empty;
            if (sai.StockAdjustment.ApprovedBy.HasValue)
            {
                userEmails.TryGetValue(sai.StockAdjustment.ApprovedBy.Value, out approvedEmail);
            }

            return new AdjustmentReportDto
            {
                AdjustmentNumber = sai.StockAdjustment.AdjNumber,
                WarehouseName = sai.StockAdjustment.Warehouse.Name,
                AdjustmentDate = sai.StockAdjustment.AdjustmentDate,
                Reason = sai.StockAdjustment.Reason ?? string.Empty,
                Status = sai.StockAdjustment.Status.ToString(),
                CreatedByEmail = createdEmail ?? "Unknown",
                ApprovedByEmail = approvedEmail ?? "-",
                ProductCode = sai.Product.Code,
                ProductName = sai.Product.Name,
                LocationBarcode = sai.Location.Barcode ?? string.Empty,
                SystemQty = sai.SystemQty,
                ActualQty = sai.ActualQty
            };
        }).ToList();

        return Result.Success(dtos);
    }
}
