using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.PickList.Commands;

public record AssignPickListCommand(Guid Id, Guid? UserId) : IRequest<Result>;

public class AssignPickListCommandHandler : IRequestHandler<AssignPickListCommand, Result>
{
    private readonly IPickListRepository _repository;

    public AssignPickListCommandHandler(IPickListRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(AssignPickListCommand request, CancellationToken cancellationToken)
    {
        var pl = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (pl == null)
        {
            return Result.Failure($"Pick list with ID '{request.Id}' was not found.");
        }

        if (pl.Status == PickListStatus.Cancelled || pl.Status == PickListStatus.Completed)
        {
            return Result.Failure($"Cannot assign a pick list that is in status: {pl.Status}");
        }

        pl.AssignedTo = request.UserId;
        if (pl.Status == PickListStatus.Pending && request.UserId.HasValue)
        {
            pl.Status = PickListStatus.InProgress;
        }

        _repository.Update(pl);
        return Result.Success();
    }
}
