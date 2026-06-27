using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.Returns.Commands;

public record CancelReturnCommand(Guid Id) : IRequest<Result>;

public class CancelReturnCommandHandler : IRequestHandler<CancelReturnCommand, Result>
{
    private readonly IReturnRepository _repository;

    public CancelReturnCommandHandler(IReturnRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(CancelReturnCommand request, CancellationToken cancellationToken)
    {
        var ret = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (ret == null)
        {
            return Result.Failure($"Return with ID '{request.Id}' was not found.");
        }

        if (ret.Status == ReturnStatus.Completed || ret.Status == ReturnStatus.Cancelled)
        {
            return Result.Failure($"Cannot cancel return in its current status: {ret.Status}");
        }

        ret.Status = ReturnStatus.Cancelled;
        _repository.Update(ret);

        return Result.Success();
    }
}
