using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.StockAdjustment.Commands;

public record SubmitStockAdjustmentCommand(Guid Id) : IRequest<Result>;

public class SubmitStockAdjustmentCommandHandler : IRequestHandler<SubmitStockAdjustmentCommand, Result>
{
    private readonly IStockAdjustmentRepository _repository;

    public SubmitStockAdjustmentCommandHandler(IStockAdjustmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(SubmitStockAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var sa = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (sa == null)
        {
            return Result.Failure($"Stock adjustment with ID '{request.Id}' was not found.");
        }

        if (sa.Status != AdjustmentStatus.Draft)
        {
            return Result.Failure($"Only draft stock adjustments can be submitted. Current status: {sa.Status}");
        }

        sa.Status = AdjustmentStatus.PendingApproval;
        _repository.Update(sa);

        return Result.Success();
    }
}
