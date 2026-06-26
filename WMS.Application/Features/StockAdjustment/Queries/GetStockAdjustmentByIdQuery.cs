using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.StockAdjustment.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.StockAdjustment.Queries;

public record GetStockAdjustmentByIdQuery(Guid Id) : IRequest<Result<StockAdjustmentDto>>;

public class GetStockAdjustmentByIdQueryHandler : IRequestHandler<GetStockAdjustmentByIdQuery, Result<StockAdjustmentDto>>
{
    private readonly IStockAdjustmentRepository _repository;

    public GetStockAdjustmentByIdQueryHandler(IStockAdjustmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<StockAdjustmentDto>> Handle(GetStockAdjustmentByIdQuery request, CancellationToken cancellationToken)
    {
        var sa = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (sa == null)
        {
            return Result.Failure<StockAdjustmentDto>($"Stock adjustment with ID '{request.Id}' was not found.");
        }

        var dto = new StockAdjustmentDto
        {
            Id = sa.Id,
            AdjNumber = sa.AdjNumber,
            WarehouseId = sa.WarehouseId,
            WarehouseName = sa.Warehouse.Name,
            AdjustmentDate = sa.AdjustmentDate,
            Reason = sa.Reason,
            ApprovedBy = sa.ApprovedBy,
            Status = sa.Status.ToString(),
            CreatedBy = sa.CreatedBy,
            CreatedAt = sa.CreatedAt,
            Items = sa.Items.Select(sai => new StockAdjustmentItemDto
            {
                Id = sai.Id,
                ProductId = sai.ProductId,
                ProductCode = sai.Product.Code,
                ProductName = sai.Product.Name,
                LocationId = sai.LocationId,
                LocationBarcode = sai.Location.Barcode,
                SystemQty = sai.SystemQty,
                ActualQty = sai.ActualQty,
                Difference = sai.Difference
            }).ToList()
        };

        return Result.Success(dto);
    }
}
