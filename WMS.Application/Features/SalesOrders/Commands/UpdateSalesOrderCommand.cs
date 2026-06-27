using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using WMS.Domain.Entities.Outbound;
using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.SalesOrders.Commands;

public record UpdateSalesOrderCommand(
    Guid Id,
    Guid CustomerId,
    DateTime? RequiredDate,
    string? Note,
    List<CreateSalesOrderItemInput> Items) : IRequest<Result>;

public class UpdateSalesOrderCommandValidator : AbstractValidator<UpdateSalesOrderCommand>
{
    public UpdateSalesOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Sales Order ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one sales order item is required.");

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

public class UpdateSalesOrderCommandHandler : IRequestHandler<UpdateSalesOrderCommand, Result>
{
    private readonly ISalesOrderRepository _repository;
    private readonly IIdGenerator _idGenerator;

    public UpdateSalesOrderCommandHandler(
        ISalesOrderRepository repository,
        IIdGenerator idGenerator)
    {
        _repository = repository;
        _idGenerator = idGenerator;
    }

    public async Task<Result> Handle(UpdateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var so = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (so == null)
        {
            return Result.Failure($"Sales order with ID '{request.Id}' was not found.");
        }

        if (so.Status != SalesOrderStatus.Draft)
        {
            return Result.Failure($"Only draft sales orders can be updated. Current status: {so.Status}");
        }

        so.CustomerId = request.CustomerId;
        so.RequiredDate = request.RequiredDate;
        so.Note = request.Note;

        // Clear existing items
        so.Items.Clear();

        // Add new items
        foreach (var itemInput in request.Items)
        {
            var item = new SalesOrderItem
            {
                Id = _idGenerator.Generate(),
                SalesOrderId = so.Id,
                ProductId = itemInput.ProductId,
                OrderedQty = itemInput.OrderedQty,
                PickedQty = 0,
                UnitPrice = itemInput.UnitPrice
            };

            so.Items.Add(item);
        }

        _repository.Update(so);

        return Result.Success();
    }
}
