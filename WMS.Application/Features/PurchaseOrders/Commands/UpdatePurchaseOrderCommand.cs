using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using WMS.Domain.Entities.Inbound;
using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.PurchaseOrders.Commands;

public record UpdatePurchaseOrderCommand(
    Guid Id,
    Guid SupplierId,
    DateTime? ExpectedDate,
    string? Note,
    List<CreatePurchaseOrderItemInput> Items) : IRequest<Result>;

public class UpdatePurchaseOrderCommandValidator : AbstractValidator<UpdatePurchaseOrderCommand>
{
    public UpdatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Purchase Order ID is required.");

        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one purchase order item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product is required.");
            item.RuleFor(x => x.OrderedQty)
                .GreaterThan(0).WithMessage("Ordered quantity must be greater than 0.");
            item.RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0.");
        });
    }
}

public class UpdatePurchaseOrderCommandHandler : IRequestHandler<UpdatePurchaseOrderCommand, Result>
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly IIdGenerator _idGenerator;

    public UpdatePurchaseOrderCommandHandler(
        IPurchaseOrderRepository repository,
        IIdGenerator idGenerator)
    {
        _repository = repository;
        _idGenerator = idGenerator;
    }

    public async Task<Result> Handle(UpdatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (po == null)
        {
            return Result.Failure($"Purchase order with ID '{request.Id}' was not found.");
        }

        if (po.Status != PurchaseOrderStatus.Draft)
        {
            return Result.Failure($"Only draft purchase orders can be updated. Current status: {po.Status}");
        }

        po.SupplierId = request.SupplierId;
        po.ExpectedDate = request.ExpectedDate;
        po.Note = request.Note;

        // Clear existing items
        po.Items.Clear();

        // Add new items
        foreach (var itemInput in request.Items)
        {
            var item = new PurchaseOrderItem
            {
                Id = _idGenerator.Generate(),
                PurchaseOrderId = po.Id,
                ProductId = itemInput.ProductId,
                OrderedQty = itemInput.OrderedQty,
                ReceivedQty = 0,
                UnitPrice = itemInput.UnitPrice
            };

            po.Items.Add(item);
        }

        _repository.Update(po);

        return Result.Success();
    }
}
