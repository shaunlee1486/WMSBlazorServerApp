using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.TransferOrders.Commands;

public record ApproveTransferOrderCommand(Guid Id) : IRequest<Result>;

public class ApproveTransferOrderCommandHandler : IRequestHandler<ApproveTransferOrderCommand, Result>
{
    private readonly ITransferOrderRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public ApproveTransferOrderCommandHandler(
        ITransferOrderRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ApproveTransferOrderCommand request, CancellationToken cancellationToken)
    {
        var to = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (to == null)
        {
            return Result.Failure($"Transfer order with ID '{request.Id}' was not found.");
        }

        if (to.Status != TransferOrderStatus.Draft)
        {
            return Result.Failure($"Only draft transfer orders can be approved. Current status: {to.Status}");
        }

        to.Status = TransferOrderStatus.Approved;
        to.ApprovedBy = _currentUserService.UserId;
        
        _repository.Update(to);

        return Result.Success();
    }
}
