using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.PickList.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.PickList.Queries;

public record GetPickListByIdQuery(Guid Id) : IRequest<Result<PickListDto>>;

public class GetPickListByIdQueryHandler : IRequestHandler<GetPickListByIdQuery, Result<PickListDto>>
{
    private readonly IPickListRepository _repository;

    public GetPickListByIdQueryHandler(IPickListRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PickListDto>> Handle(GetPickListByIdQuery request, CancellationToken cancellationToken)
    {
        var pl = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (pl == null)
        {
            return Result.Failure<PickListDto>($"Pick list with ID '{request.Id}' was not found.");
        }

        var dto = new PickListDto
        {
            Id = pl.Id,
            PickListNumber = pl.PickListNumber,
            SalesOrderId = pl.SalesOrderId,
            SONumber = pl.SalesOrder.SONumber,
            CustomerName = pl.SalesOrder.Customer.Name,
            AssignedTo = pl.AssignedTo,
            Status = pl.Status.ToString(),
            CreatedAt = pl.CreatedAt,
            Items = pl.Items.Select(pli => new PickListItemDto
            {
                Id = pli.Id,
                ProductId = pli.ProductId,
                ProductCode = pli.Product.Code,
                ProductName = pli.Product.Name,
                LocationId = pli.LocationId,
                LocationBarcode = pli.Location.Barcode ?? string.Empty,
                LocationName = $"{pli.Location.Aisle}-{pli.Location.Bay}-{pli.Location.Level}-{pli.Location.Position}",
                RequiredQty = pli.RequiredQty,
                PickedQty = pli.PickedQty,
                Status = pli.Status
            }).ToList()
        };

        return Result.Success(dto);
    }
}
