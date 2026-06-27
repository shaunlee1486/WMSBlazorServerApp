using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using WMS.Domain.Entities.Internal;
using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.TransferOrders.Commands;

public record UpdateTransferOrderItemInput(Guid ProductId, Guid FromLocationId, Guid ToLocationId, decimal Qty);

public record UpdateTransferOrderCommand(
    Guid Id,
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    List<UpdateTransferOrderItemInput> Items) : IRequest<Result>;

public class UpdateTransferOrderCommandValidator : AbstractValidator<UpdateTransferOrderCommand>
{
    public UpdateTransferOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Transfer order ID is required.");
        
        RuleFor(x => x.FromWarehouseId)
            .NotEmpty().WithMessage("Source warehouse is required.");

        RuleFor(x => x.ToWarehouseId)
            .NotEmpty().WithMessage("Destination warehouse is required.")
            .NotEqual(x => x.FromWarehouseId).WithMessage("Source and destination warehouses must be different.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one transfer item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product is required.");
            item.RuleFor(x => x.FromLocationId).NotEmpty().WithMessage("Source location is required.");
            item.RuleFor(x => x.ToLocationId).NotEmpty().WithMessage("Destination location is required.")
                .NotEqual(x => x.FromLocationId).WithMessage("Source and destination locations must be different.");
            item.RuleFor(x => x.Qty)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        });
    }
}

public class UpdateTransferOrderCommandHandler : IRequestHandler<UpdateTransferOrderCommand, Result>
{
    private readonly ITransferOrderRepository _repository;
    private readonly IIdGenerator _idGenerator;

    public UpdateTransferOrderCommandHandler(
        ITransferOrderRepository repository,
        IIdGenerator idGenerator)
    {
        _repository = repository;
        _idGenerator = idGenerator;
    }

    public async Task<Result> Handle(UpdateTransferOrderCommand request, CancellationToken cancellationToken)
    {
        var to = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (to == null)
        {
            return Result.Failure($"Transfer order with ID '{request.Id}' was not found.");
        }

        if (to.Status != TransferOrderStatus.Draft)
        {
            return Result.Failure($"Only draft transfer orders can be updated. Current status: {to.Status}");
        }

        to.FromWarehouseId = request.FromWarehouseId;
        to.ToWarehouseId = request.ToWarehouseId;
        to.Items.Clear();

        foreach (var itemInput in request.Items)
        {
            var item = new TransferOrderItem
            {
                Id = _idGenerator.Generate(),
                TransferOrderId = to.Id,
                ProductId = itemInput.ProductId,
                FromLocationId = itemInput.FromLocationId,
                ToLocationId = itemInput.ToLocationId,
                Qty = itemInput.Qty,
                Status = TransferOrderItemStatus.Pending
            };

            to.Items.Add(item);
        }

        _repository.Update(to);

        return Result.Success();
    }
}
