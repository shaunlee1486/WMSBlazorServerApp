using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.GoodsIssue.Commands;

public record CancelGoodsIssueCommand(Guid Id) : IRequest<Result>;

public class CancelGoodsIssueCommandHandler : IRequestHandler<CancelGoodsIssueCommand, Result>
{
    private readonly IGoodsIssueRepository _repository;

    public CancelGoodsIssueCommandHandler(IGoodsIssueRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(CancelGoodsIssueCommand request, CancellationToken cancellationToken)
    {
        var gi = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (gi == null)
        {
            return Result.Failure($"Goods issue with ID '{request.Id}' was not found.");
        }

        if (gi.Status != GoodsIssueStatus.Draft)
        {
            return Result.Failure($"Only draft goods issues can be cancelled. Current status: {gi.Status}");
        }

        gi.Status = GoodsIssueStatus.Cancelled;
        _repository.Update(gi);

        return Result.Success();
    }
}
