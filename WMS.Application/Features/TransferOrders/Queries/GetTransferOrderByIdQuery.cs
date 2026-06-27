using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.TransferOrders.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.TransferOrders.Queries;

public record GetTransferOrderByIdQuery(Guid Id) : IRequest<Result<TransferOrderDto>>;

public class GetTransferOrderByIdQueryHandler : IRequestHandler<GetTransferOrderByIdQuery, Result<TransferOrderDto>>
{
    private readonly ITransferOrderRepository _repository;

    public GetTransferOrderByIdQueryHandler(ITransferOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TransferOrderDto>> Handle(GetTransferOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var to = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (to == null)
        {
            return Result.Failure<TransferOrderDto>($"Transfer order with ID '{request.Id}' was not found.");
        }

        var dto = new TransferOrderDto
        {
            Id = to.Id,
            TONumber = to.TONumber,
            FromWarehouseId = to.FromWarehouseId,
            FromWarehouseName = to.FromWarehouse.Name,
            ToWarehouseId = to.ToWarehouseId,
            ToWarehouseName = to.ToWarehouse.Name,
            Status = to.Status.ToString(),
            RequestedBy = to.RequestedBy,
            ApprovedBy = to.ApprovedBy,
            CreatedAt = to.CreatedAt,
            Items = to.Items.Select(toi => new TransferOrderItemDto
            {
                Id = toi.Id,
                ProductId = toi.ProductId,
                ProductCode = toi.Product.Code,
                ProductName = toi.Product.Name,
                FromLocationId = toi.FromLocationId,
                FromLocationBarcode = toi.FromLocation.Barcode ?? string.Empty,
                ToLocationId = toi.ToLocationId,
                ToLocationBarcode = toi.ToLocation.Barcode ?? string.Empty,
                Qty = toi.Qty,
                Status = toi.Status.ToString()
            }).ToList()
        };

        return Result.Success(dto);
    }
}
