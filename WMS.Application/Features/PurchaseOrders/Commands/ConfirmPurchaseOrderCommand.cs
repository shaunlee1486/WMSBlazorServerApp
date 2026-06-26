using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.PurchaseOrders.Commands;

public record ConfirmPurchaseOrderCommand(Guid Id) : IRequest<Result>;

public class ConfirmPurchaseOrderCommandHandler : IRequestHandler<ConfirmPurchaseOrderCommand, Result>
{
    private readonly IPurchaseOrderRepository _repository;

    public ConfirmPurchaseOrderCommandHandler(IPurchaseOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(ConfirmPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (po == null)
        {
            return Result.Failure($"Purchase order with ID '{request.Id}' was not found.");
        }

        if (po.Status != PurchaseOrderStatus.Draft)
        {
            return Result.Failure($"Only draft purchase orders can be confirmed. Current status: {po.Status}");
        }

        po.Status = PurchaseOrderStatus.Confirmed;
        _repository.Update(po);

        return Result.Success();
    }
}
