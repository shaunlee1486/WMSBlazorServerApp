using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WMS.Domain.Entities.Inventory;
using WMS.Domain.Entities.Reporting;
using WMS.SharedKernel;

namespace WMS.Domain.Interfaces.Repositories;

public interface IStockRepository : IRepository<Stock>
{
    Task<PagedResult<Stock>> GetStockOverviewPagedAsync(
        string? searchTerm,
        Guid? warehouseId,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Stock>> GetLowStockReportAsync(CancellationToken cancellationToken = default);

    Task<int> GetLowStockCountAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockCategoryStats>> GetStockCategoryStatsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Stock>> GetStockSnapshotReportAsync(Guid? warehouseId, Guid? categoryId, string? searchTerm, CancellationToken cancellationToken = default);
}
