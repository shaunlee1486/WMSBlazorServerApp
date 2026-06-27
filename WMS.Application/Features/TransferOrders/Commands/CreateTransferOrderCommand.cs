using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Entities.Internal;
using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.TransferOrders.Commands;

public record CreateTransferOrderItemInput(Guid ProductId, Guid FromLocationId, Guid ToLocationId, decimal Qty);

public record CreateTransferOrderCommand(
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    List<CreateTransferOrderItemInput> Items) : IRequest<Result<Guid>>;

public class CreateTransferOrderCommandValidator : AbstractValidator<CreateTransferOrderCommand>
{
    public CreateTransferOrderCommandValidator()
    {
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

public class CreateTransferOrderCommandHandler : IRequestHandler<CreateTransferOrderCommand, Result<Guid>>
{
    private readonly ITransferOrderRepository _repository;
    private readonly IIdGenerator _idGenerator;
    private readonly ICurrentUserService _currentUserService;

    public CreateTransferOrderCommandHandler(
        ITransferOrderRepository repository,
        IIdGenerator idGenerator,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _idGenerator = idGenerator;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateTransferOrderCommand request, CancellationToken cancellationToken)
    {
        var nextTONumber = await _repository.GetNextTONumberAsync(cancellationToken);
        var createdBy = _currentUserService.UserId ?? Guid.Empty;

        var to = new TransferOrder
        {
            Id = _idGenerator.Generate(),
            TONumber = nextTONumber,
            FromWarehouseId = request.FromWarehouseId,
            ToWarehouseId = request.ToWarehouseId,
            Status = TransferOrderStatus.Draft,
            RequestedBy = createdBy
        };

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

        await _repository.AddAsync(to, cancellationToken);

        return Result.Success(to.Id);
    }
}
