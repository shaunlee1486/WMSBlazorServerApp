using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Domain.Entities.MasterData;
using WMS.Domain.Interfaces;
using WMS.SharedKernel;

namespace WMS.Application.Features.Warehouses.Commands;

public record DeleteWarehouseCommand(Guid Id) : IRequest<Result>;

public class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand, Result>
{
    private readonly IRepository<Warehouse> _repository;

    public DeleteWarehouseCommandHandler(IRepository<Warehouse> repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse == null)
        {
            return Result.Failure($"Warehouse with ID '{request.Id}' was not found.");
        }

        _repository.Delete(warehouse);

        return Result.Success();
    }
}
