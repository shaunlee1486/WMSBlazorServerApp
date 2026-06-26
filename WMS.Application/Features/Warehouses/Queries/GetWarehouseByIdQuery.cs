using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.Warehouses.DTOs;
using WMS.Domain.Entities.MasterData;
using WMS.Domain.Interfaces;
using WMS.SharedKernel;

namespace WMS.Application.Features.Warehouses.Queries;

public record GetWarehouseByIdQuery(Guid Id) : IRequest<Result<WarehouseDetailDto>>;

public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, Result<WarehouseDetailDto>>
{
    private readonly IRepository<Warehouse> _repository;

    public GetWarehouseByIdQueryHandler(IRepository<Warehouse> repository)
    {
        _repository = repository;
    }

    public async Task<Result<WarehouseDetailDto>> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse == null)
        {
            return Result.Failure<WarehouseDetailDto>($"Warehouse with ID '{request.Id}' was not found.");
        }

        var dto = new WarehouseDetailDto
        {
            Id = warehouse.Id,
            Code = warehouse.Code,
            Name = warehouse.Name,
            Address = warehouse.Address,
            IsActive = warehouse.IsActive,
            CreatedAt = warehouse.CreatedAt,
            UpdatedAt = warehouse.UpdatedAt
        };

        return Result.Success(dto);
    }
}
