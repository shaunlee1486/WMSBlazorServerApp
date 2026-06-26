using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Domain.Entities.MasterData;
using WMS.Domain.Interfaces;
using WMS.SharedKernel;

namespace WMS.Application.Features.Warehouses.Commands;

public record ToggleWarehouseStatusCommand(Guid Id) : IRequest<Result>;

public class ToggleWarehouseStatusCommandHandler : IRequestHandler<ToggleWarehouseStatusCommand, Result>
{
    private readonly IRepository<Warehouse> _repository;

    public ToggleWarehouseStatusCommandHandler(IRepository<Warehouse> repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(ToggleWarehouseStatusCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse == null)
        {
            return Result.Failure($"Warehouse with ID '{request.Id}' was not found.");
        }

        warehouse.IsActive = !warehouse.IsActive;
        _repository.Update(warehouse);

        return Result.Success();
    }
}
