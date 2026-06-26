using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.PurchaseOrders.Commands;

public record CancelPurchaseOrderCommand(Guid Id) : IRequest<Result>;

public class CancelPurchaseOrderCommandHandler : IRequestHandler<CancelPurchaseOrderCommand, Result>
{
    private readonly IPurchaseOrderRepository _repository;

    public CancelPurchaseOrderCommandHandler(IPurchaseOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (po == null)
        {
            return Result.Failure($"Purchase order with ID '{request.Id}' was not found.");
        }

        if (po.Status != PurchaseOrderStatus.Draft && po.Status != PurchaseOrderStatus.Confirmed)
        {
            return Result.Failure($"Only draft or confirmed purchase orders can be cancelled. Current status: {po.Status}");
        }

        po.Status = PurchaseOrderStatus.Cancelled;
        _repository.Update(po);

        return Result.Success();
    }
}
