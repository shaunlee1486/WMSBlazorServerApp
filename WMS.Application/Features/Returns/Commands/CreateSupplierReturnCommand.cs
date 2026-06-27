using System;
using System.Collections.Generic;
using System.Linq;
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

public record CreateReturnItemInput(Guid ProductId, Guid LocationId, decimal Quantity, string? Note);

public record CreateSupplierReturnCommand(
    string ReferenceNo,
    string? Note,
    List<CreateReturnItemInput> Items) : IRequest<Result<Guid>>;

public class CreateSupplierReturnCommandValidator : AbstractValidator<CreateSupplierReturnCommand>
{
    public CreateSupplierReturnCommandValidator()
    {
        RuleFor(x => x.ReferenceNo)
            .NotEmpty().WithMessage("Original Goods Receipt reference number is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one return item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product is required.");
            item.RuleFor(x => x.LocationId).NotEmpty().WithMessage("Location is required.");
            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        });
    }
}

public class CreateSupplierReturnCommandHandler : IRequestHandler<CreateSupplierReturnCommand, Result<Guid>>
{
    private readonly IReturnRepository _repository;
    private readonly IStockRepository _stockRepository;
    private readonly IIdGenerator _idGenerator;
    private readonly ICurrentUserService _currentUserService;

    public CreateSupplierReturnCommandHandler(
        IReturnRepository repository,
        IStockRepository stockRepository,
        IIdGenerator idGenerator,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _stockRepository = stockRepository;
        _idGenerator = idGenerator;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateSupplierReturnCommand request, CancellationToken cancellationToken)
    {
        var nextReturnNumber = await _repository.GetNextReturnNumberAsync(cancellationToken);
        var createdBy = _currentUserService.UserId ?? Guid.Empty;

        // 1. Verify stock availability at the locations
        foreach (var itemInput in request.Items)
        {
            var stocks = await _stockRepository.FindAsync(s => s.ProductId == itemInput.ProductId && s.LocationId == itemInput.LocationId, cancellationToken);
            var stock = stocks.FirstOrDefault();
            if (stock == null)
            {
                return Result.Failure<Guid>($"Stock not found for product ID '{itemInput.ProductId}' at location '{itemInput.LocationId}'.");
            }

            var available = stock.Quantity - stock.ReservedQuantity;
            if (available < itemInput.Quantity)
            {
                return Result.Failure<Guid>($"Insufficient stock for product ID '{itemInput.ProductId}' at location '{itemInput.LocationId}'. Required: {itemInput.Quantity}, Available: {available}.");
            }
        }

        var ret = new Return
        {
            Id = _idGenerator.Generate(),
            ReturnNumber = nextReturnNumber,
            ReturnType = ReturnType.Supplier,
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
                InspectionStatus = InspectionStatus.Accepted, // Supplier returns do not need inspection, pre-accept
                Note = itemInput.Note
            };

            ret.Items.Add(item);
        }

        await _repository.AddAsync(ret, cancellationToken);

        return Result.Success(ret.Id);
    }
}
