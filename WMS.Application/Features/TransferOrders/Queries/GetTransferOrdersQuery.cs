using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.TransferOrders.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.TransferOrders.Queries;

public record GetTransferOrdersQuery(
    string? SearchTerm,
    string? Status,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IRequest<PagedResult<TransferOrderDto>>;

public class GetTransferOrdersQueryHandler : IRequestHandler<GetTransferOrdersQuery, PagedResult<TransferOrderDto>>
{
    private readonly ITransferOrderRepository _repository;

    public GetTransferOrdersQueryHandler(ITransferOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<TransferOrderDto>> Handle(GetTransferOrdersQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await _repository.GetTransferOrdersPagedAsync(
            request.SearchTerm,
            request.Status,
            request.SortColumn,
            request.SortOrder,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = pagedResult.Items.Select(to => new TransferOrderDto
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
            CreatedAt = to.CreatedAt
        }).ToList();

        return new PagedResult<TransferOrderDto>(dtos, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize);
    }
}
