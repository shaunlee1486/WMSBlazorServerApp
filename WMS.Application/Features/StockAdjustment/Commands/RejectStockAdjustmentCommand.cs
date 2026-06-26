using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.StockAdjustment.Commands;

public record RejectStockAdjustmentCommand(Guid Id, string? Reason) : IRequest<Result>;

public class RejectStockAdjustmentCommandHandler : IRequestHandler<RejectStockAdjustmentCommand, Result>
{
    private readonly IStockAdjustmentRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public RejectStockAdjustmentCommandHandler(
        IStockAdjustmentRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(RejectStockAdjustmentCommand request, CancellationToken cancellationToken)
    {
        // Enforce role check: WarehouseManager or Admin
        var isAuthorized = _currentUserService.Roles.Any(role => 
            role.Equals("WarehouseManager", StringComparison.OrdinalIgnoreCase) || 
            role.Equals("Admin", StringComparison.OrdinalIgnoreCase));

        if (!isAuthorized)
        {
            return Result.Failure("Unauthorized. Only Warehouse Managers or Administrators can reject stock adjustments.");
        }

        var sa = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (sa == null)
        {
            return Result.Failure($"Stock adjustment with ID '{request.Id}' was not found.");
        }

        if (sa.Status != AdjustmentStatus.PendingApproval)
        {
            return Result.Failure($"Only stock adjustments in PendingApproval status can be rejected. Current status: {sa.Status}");
        }

        sa.Status = AdjustmentStatus.Rejected;
        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            sa.Reason = string.IsNullOrWhiteSpace(sa.Reason) 
                ? $"Rejected: {request.Reason}" 
                : $"{sa.Reason} | Rejected: {request.Reason}";
        }
        
        _repository.Update(sa);

        return Result.Success();
    }
}
