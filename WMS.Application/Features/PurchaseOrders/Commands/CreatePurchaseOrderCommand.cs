using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Entities.Inbound;
using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.PurchaseOrders.Commands;

public record CreatePurchaseOrderItemInput(Guid ProductId, decimal OrderedQty, decimal UnitPrice);

public record CreatePurchaseOrderCommand(
    Guid SupplierId,
    DateTime? ExpectedDate,
    string? Note,
    List<CreatePurchaseOrderItemInput> Items) : IRequest<Result<Guid>>;

public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
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

public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, Result<Guid>>
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly IIdGenerator _idGenerator;
    private readonly ICurrentUserService _currentUserService;

    public CreatePurchaseOrderCommandHandler(
        IPurchaseOrderRepository repository,
        IIdGenerator idGenerator,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _idGenerator = idGenerator;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var nextPONumber = await _repository.GetNextPONumberAsync(cancellationToken);
        var createdBy = _currentUserService.UserId ?? Guid.Empty;

        var po = new PurchaseOrder
        {
            Id = _idGenerator.Generate(),
            PONumber = nextPONumber,
            SupplierId = request.SupplierId,
            ExpectedDate = request.ExpectedDate,
            Note = request.Note,
            Status = PurchaseOrderStatus.Draft,
            CreatedBy = createdBy
        };

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

        await _repository.AddAsync(po, cancellationToken);

        return Result.Success(po.Id);
    }
}
