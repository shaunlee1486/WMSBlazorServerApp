using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.PickList.Commands;

public record UpdatePickedQtyCommand(
    Guid PickListId,
    Guid PickListItemId,
    decimal PickedQty) : IRequest<Result>;

public class UpdatePickedQtyCommandValidator : AbstractValidator<UpdatePickedQtyCommand>
{
    public UpdatePickedQtyCommandValidator()
    {
        RuleFor(x => x.PickListId).NotEmpty();
        RuleFor(x => x.PickListItemId).NotEmpty();
        RuleFor(x => x.PickedQty).GreaterThanOrEqualTo(0).WithMessage("Picked quantity cannot be negative.");
    }
}

public class UpdatePickedQtyCommandHandler : IRequestHandler<UpdatePickedQtyCommand, Result>
{
    private readonly IPickListRepository _repository;

    public UpdatePickedQtyCommandHandler(IPickListRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdatePickedQtyCommand request, CancellationToken cancellationToken)
    {
        var pl = await _repository.GetByIdWithItemsAsync(request.PickListId, cancellationToken);
        if (pl == null)
        {
            return Result.Failure($"Pick list with ID '{request.PickListId}' was not found.");
        }

        if (pl.Status != PickListStatus.InProgress)
        {
            return Result.Failure($"Picked quantities can only be updated for pick lists in progress. Current status: {pl.Status}");
        }

        var item = pl.Items.FirstOrDefault(i => i.Id == request.PickListItemId);
        if (item == null)
        {
            return Result.Failure($"Pick list item with ID '{request.PickListItemId}' was not found.");
        }

        if (request.PickedQty > item.RequiredQty)
        {
            return Result.Failure($"Picked quantity ({request.PickedQty}) cannot exceed required quantity ({item.RequiredQty}).");
        }

        item.PickedQty = request.PickedQty;
        item.Status = item.PickedQty >= item.RequiredQty ? "Completed" : "Pending";

        _repository.Update(pl);
        return Result.Success();
    }
}
