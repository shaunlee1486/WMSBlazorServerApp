using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using WMS.Domain.Entities.MasterData;
using WMS.Domain.Interfaces;
using WMS.SharedKernel;

namespace WMS.Application.Features.Warehouses.Commands;

public record CreateWarehouseCommand(string Code, string Name, string? Address) : IRequest<Result<Guid>>;

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Warehouse code is required.")
            .MaximumLength(50).WithMessage("Warehouse code must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Warehouse name is required.")
            .MaximumLength(200).WithMessage("Warehouse name must not exceed 200 characters.");
    }
}

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, Result<Guid>>
{
    private readonly IRepository<Warehouse> _repository;
    private readonly IIdGenerator _idGenerator;

    public CreateWarehouseCommandHandler(IRepository<Warehouse> repository, IIdGenerator idGenerator)
    {
        _repository = repository;
        _idGenerator = idGenerator;
    }

    public async Task<Result<Guid>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        // Enforce business rule: warehouse code uniqueness
        var existing = await _repository.FindAsync(w => w.Code == request.Code, cancellationToken);
        if (existing.Count > 0)
        {
            return Result.Failure<Guid>($"A warehouse with code '{request.Code}' already exists.");
        }

        var warehouse = new Warehouse
        {
            Id = _idGenerator.Generate(),
            Code = request.Code,
            Name = request.Name,
            Address = request.Address,
            IsActive = true
        };

        await _repository.AddAsync(warehouse, cancellationToken);

        return Result.Success(warehouse.Id);
    }
}
