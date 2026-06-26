using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.Stock.DTOs;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Application.Features.Stock.Queries;

public record GetStockMovementsQuery(
    string? SearchTerm,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<StockMovementDto>>>;

public class GetStockMovementsQueryHandler : IRequestHandler<GetStockMovementsQuery, Result<PagedResult<StockMovementDto>>>
{
    private readonly IStockMovementRepository _stockMovementRepository;

    public GetStockMovementsQueryHandler(IStockMovementRepository stockMovementRepository)
    {
        _stockMovementRepository = stockMovementRepository;
    }

    public async Task<Result<PagedResult<StockMovementDto>>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await _stockMovementRepository.GetStockMovementsPagedAsync(
            request.SearchTerm,
            request.SortColumn,
            request.SortOrder,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = pagedResult.Items.Select(sm => new StockMovementDto
        {
            Id = sm.Id,
            ProductCode = sm.Product.Code,
            ProductName = sm.Product.Name,
            FromLocationBarcode = sm.FromLocation?.Barcode,
            ToLocationBarcode = sm.ToLocation?.Barcode,
            Quantity = sm.Quantity,
            MovementType = sm.MovementType.ToString(),
            ReferenceNo = sm.ReferenceNo,
            Note = sm.Note,
            CreatedAt = sm.CreatedAt
        }).ToList();

        var result = new PagedResult<StockMovementDto>(dtos, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize);
        return Result.Success(result);
    }
}
