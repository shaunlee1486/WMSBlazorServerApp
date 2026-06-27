using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WMS.Application.Features.Dashboard.DTOs;
using WMS.Domain.Entities.MasterData;
using WMS.Domain.Interfaces;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Application.Features.Dashboard.Queries;

public record GetDashboardStatsQuery : IRequest<Result<DashboardStatsDto>>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IRepository<Product> _productRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IPurchaseOrderRepository _poRepository;
    private readonly ISalesOrderRepository _soRepository;

    public GetDashboardStatsQueryHandler(
        IRepository<Product> productRepository,
        IStockRepository stockRepository,
        IPurchaseOrderRepository poRepository,
        ISalesOrderRepository soRepository)
    {
        _productRepository = productRepository;
        _stockRepository = stockRepository;
        _poRepository = poRepository;
        _soRepository = soRepository;
    }

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var activeProducts = await _productRepository.FindAsync(p => p.IsActive, cancellationToken);
        var totalProducts = activeProducts.Count;

        var lowStockCount = await _stockRepository.GetLowStockCountAsync(cancellationToken);

        var pendingPOs = await _poRepository.FindAsync(
            po => po.Status != PurchaseOrderStatus.FullyReceived && po.Status != PurchaseOrderStatus.Cancelled,
            cancellationToken);
        var pendingPOsCount = pendingPOs.Count;

        var openSOs = await _soRepository.FindAsync(
            so => so.Status != SalesOrderStatus.Shipped && so.Status != SalesOrderStatus.Cancelled,
            cancellationToken);
        var openSOsCount = openSOs.Count;

        var dto = new DashboardStatsDto
        {
            TotalProducts = totalProducts,
            LowStockCount = lowStockCount,
            PendingPOsCount = pendingPOsCount,
            OpenSOsCount = openSOsCount
        };

        return Result.Success(dto);
    }
}
