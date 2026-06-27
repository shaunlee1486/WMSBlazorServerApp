using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.GoodsIssue.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.GoodsIssue.Queries;

public record GetGoodsIssueByIdQuery(Guid Id) : IRequest<Result<GoodsIssueDto>>;

public class GetGoodsIssueByIdQueryHandler : IRequestHandler<GetGoodsIssueByIdQuery, Result<GoodsIssueDto>>
{
    private readonly IGoodsIssueRepository _repository;

    public GetGoodsIssueByIdQueryHandler(IGoodsIssueRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GoodsIssueDto>> Handle(GetGoodsIssueByIdQuery request, CancellationToken cancellationToken)
    {
        var gi = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (gi == null)
        {
            return Result.Failure<GoodsIssueDto>($"Goods issue with ID '{request.Id}' was not found.");
        }

        var dto = new GoodsIssueDto
        {
            Id = gi.Id,
            GINumber = gi.GINumber,
            SalesOrderId = gi.SalesOrderId,
            SONumber = gi.SalesOrder.SONumber,
            CustomerName = gi.SalesOrder.Customer.Name,
            IssuedDate = gi.IssuedDate,
            IssuedBy = gi.IssuedBy,
            Status = gi.Status.ToString(),
            Note = gi.Note,
            CreatedAt = gi.CreatedAt,
            Items = gi.Items.Select(gii => new GoodsIssueItemDto
            {
                Id = gii.Id,
                ProductId = gii.ProductId,
                ProductCode = gii.Product.Code,
                ProductName = gii.Product.Name,
                LocationId = gii.LocationId,
                LocationBarcode = gii.Location.Barcode ?? string.Empty,
                LocationName = $"{gii.Location.Aisle}-{gii.Location.Bay}-{gii.Location.Level}-{gii.Location.Position}",
                IssuedQty = gii.IssuedQty,
                BatchNo = gii.BatchNo
            }).ToList()
        };

        return Result.Success(dto);
    }
}
