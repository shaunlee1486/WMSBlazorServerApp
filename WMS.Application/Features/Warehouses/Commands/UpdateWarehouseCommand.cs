using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using WMS.Domain.Entities.MasterData;
using WMS.Domain.Interfaces;
using WMS.SharedKernel;

namespace WMS.Application.Features.Warehouses.Commands;

public record UpdateWarehouseCommand(Guid Id, string Code, string Name, string? Address) : IRequest<Result>;

public class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Warehouse code is required.")
            .MaximumLength(50).WithMessage("Warehouse code must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Warehouse name is required.")
            .MaximumLength(200).WithMessage("Warehouse name must not exceed 200 characters.");
    }
}

public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, Result>
{
    private readonly IRepository<Warehouse> _repository;

    public UpdateWarehouseCommandHandler(IRepository<Warehouse> repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse == null)
        {
            return Result.Failure($"Warehouse with ID '{request.Id}' was not found.");
        }

        // Validate code uniqueness if changing
        if (warehouse.Code != request.Code)
        {
            var existing = await _repository.FindAsync(w => w.Code == request.Code, cancellationToken);
            if (existing.Count > 0)
            {
                return Result.Failure($"A warehouse with code '{request.Code}' already exists.");
            }
        }

        warehouse.Code = request.Code;
        warehouse.Name = request.Name;
        warehouse.Address = request.Address;

        _repository.Update(warehouse);

        return Result.Success();
    }
}
