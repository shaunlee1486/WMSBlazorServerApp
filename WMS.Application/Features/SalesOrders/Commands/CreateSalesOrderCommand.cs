using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Entities.Outbound;
using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.SalesOrders.Commands;

public record CreateSalesOrderItemInput(Guid ProductId, decimal OrderedQty, decimal UnitPrice);

public record CreateSalesOrderCommand(
    Guid CustomerId,
    DateTime? RequiredDate,
    string? Note,
    List<CreateSalesOrderItemInput> Items) : IRequest<Result<Guid>>;

public class CreateSalesOrderCommandValidator : AbstractValidator<CreateSalesOrderCommand>
{
    public CreateSalesOrderCommandValidator()
    {
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

public class CreateSalesOrderCommandHandler : IRequestHandler<CreateSalesOrderCommand, Result<Guid>>
{
    private readonly ISalesOrderRepository _repository;
    private readonly IIdGenerator _idGenerator;
    private readonly ICurrentUserService _currentUserService;

    public CreateSalesOrderCommandHandler(
        ISalesOrderRepository repository,
        IIdGenerator idGenerator,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _idGenerator = idGenerator;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var nextSONumber = await _repository.GetNextSONumberAsync(cancellationToken);
        var createdBy = _currentUserService.UserId ?? Guid.Empty;

        var so = new SalesOrder
        {
            Id = _idGenerator.Generate(),
            SONumber = nextSONumber,
            CustomerId = request.CustomerId,
            RequiredDate = request.RequiredDate,
            Note = request.Note,
            Status = SalesOrderStatus.Draft,
            CreatedBy = createdBy
        };

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

        await _repository.AddAsync(so, cancellationToken);

        return Result.Success(so.Id);
    }
}
