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

namespace WMS.Application.Features.Returns.Commands;

public record CreateCustomerReturnCommand(
    string ReferenceNo, // Reference to Sales Order or Goods Issue
    string? Note,
    List<CreateReturnItemInput> Items) : IRequest<Result<Guid>>;

public class CreateCustomerReturnCommandValidator : AbstractValidator<CreateCustomerReturnCommand>
{
    public CreateCustomerReturnCommandValidator()
    {
        RuleFor(x => x.ReferenceNo)
            .NotEmpty().WithMessage("Original Goods Issue reference number is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one return item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product is required.");
            item.RuleFor(x => x.LocationId).NotEmpty().WithMessage("Target location is required.");
            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        });
    }
}

public class CreateCustomerReturnCommandHandler : IRequestHandler<CreateCustomerReturnCommand, Result<Guid>>
{
    private readonly IReturnRepository _repository;
    private readonly IIdGenerator _idGenerator;
    private readonly ICurrentUserService _currentUserService;

    public CreateCustomerReturnCommandHandler(
        IReturnRepository repository,
        IIdGenerator idGenerator,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _idGenerator = idGenerator;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateCustomerReturnCommand request, CancellationToken cancellationToken)
    {
        var nextReturnNumber = await _repository.GetNextReturnNumberAsync(cancellationToken);
        var createdBy = _currentUserService.UserId ?? Guid.Empty;

        var ret = new Return
        {
            Id = _idGenerator.Generate(),
            ReturnNumber = nextReturnNumber,
            ReturnType = ReturnType.Customer,
            ReferenceNo = request.ReferenceNo,
            Status = ReturnStatus.Pending,
            Note = request.Note,
            CreatedBy = createdBy
        };

        foreach (var itemInput in request.Items)
        {
            var item = new ReturnItem
            {
                Id = _idGenerator.Generate(),
                ReturnId = ret.Id,
                ProductId = itemInput.ProductId,
                LocationId = itemInput.LocationId,
                Quantity = itemInput.Quantity,
                InspectionStatus = InspectionStatus.Pending, // Customer return items default to Pending
                Note = itemInput.Note
            };

            ret.Items.Add(item);
        }

        await _repository.AddAsync(ret, cancellationToken);

        return Result.Success(ret.Id);
    }
}
