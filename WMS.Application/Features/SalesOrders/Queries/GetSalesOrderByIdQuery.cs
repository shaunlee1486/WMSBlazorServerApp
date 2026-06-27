using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.SalesOrders.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.SalesOrders.Queries;

public record GetSalesOrderByIdQuery(Guid Id) : IRequest<Result<SalesOrderDto>>;

public class GetSalesOrderByIdQueryHandler : IRequestHandler<GetSalesOrderByIdQuery, Result<SalesOrderDto>>
{
    private readonly ISalesOrderRepository _repository;

    public GetSalesOrderByIdQueryHandler(ISalesOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<SalesOrderDto>> Handle(GetSalesOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var so = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (so == null)
        {
            return Result.Failure<SalesOrderDto>($"Sales order with ID '{request.Id}' was not found.");
        }

        var dto = new SalesOrderDto
        {
            Id = so.Id,
            SONumber = so.SONumber,
            CustomerId = so.CustomerId,
            CustomerName = so.Customer.Name,
            Status = so.Status.ToString(),
            OrderDate = so.OrderDate,
            RequiredDate = so.RequiredDate,
            Note = so.Note,
            CreatedBy = so.CreatedBy,
            CreatedAt = so.CreatedAt,
            Items = so.Items.Select(soi => new SalesOrderItemDto
            {
                Id = soi.Id,
                ProductId = soi.ProductId,
                ProductCode = soi.Product.Code,
                ProductName = soi.Product.Name,
                OrderedQty = soi.OrderedQty,
                PickedQty = soi.PickedQty,
                UnitPrice = soi.UnitPrice
            }).ToList()
        };

        return Result.Success(dto);
    }
}
