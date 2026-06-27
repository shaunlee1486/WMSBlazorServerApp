using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.Returns.Commands;

public record UpdateReturnInspectionStatusCommand(
    Guid ReturnId,
    Guid ReturnItemId,
    InspectionStatus Status,
    string? Note) : IRequest<Result>;

public class UpdateReturnInspectionStatusCommandHandler : IRequestHandler<UpdateReturnInspectionStatusCommand, Result>
{
    private readonly IReturnRepository _repository;

    public UpdateReturnInspectionStatusCommandHandler(IReturnRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateReturnInspectionStatusCommand request, CancellationToken cancellationToken)
    {
        var ret = await _repository.GetByIdWithItemsAsync(request.ReturnId, cancellationToken);
        if (ret == null)
        {
            return Result.Failure($"Return with ID '{request.ReturnId}' was not found.");
        }

        if (ret.Status != ReturnStatus.Pending && ret.Status != ReturnStatus.Processing)
        {
            return Result.Failure($"Cannot update item inspection status on a return with status: {ret.Status}");
        }

        var item = ret.Items.FirstOrDefault(ri => ri.Id == request.ReturnItemId);
        if (item == null)
        {
            return Result.Failure($"Return item with ID '{request.ReturnItemId}' was not found on this return.");
        }

        item.InspectionStatus = request.Status;
        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            item.Note = request.Note;
        }

        // If any item was inspected, transition Return status to Processing if it's currently Pending
        if (ret.Status == ReturnStatus.Pending)
        {
            ret.Status = ReturnStatus.Processing;
        }

        _repository.Update(ret);

        return Result.Success();
    }
}
