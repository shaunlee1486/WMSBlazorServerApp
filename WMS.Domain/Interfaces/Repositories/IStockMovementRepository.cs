using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WMS.Domain.Entities.Inventory;
using WMS.Domain.Entities.Reporting;
using WMS.SharedKernel;

namespace WMS.Domain.Interfaces.Repositories;

public interface IStockMovementRepository : IRepository<StockMovement>
{
    Task<PagedResult<StockMovement>> GetStockMovementsPagedAsync(
        string? searchTerm,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductMovementStats>> GetTopProductsStatsAsync(int count, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockMovement>> GetMovementHistoryReportAsync(Guid? productId, Guid? locationId, string? movementType, DateTime? startDate, DateTime? endDate, string? searchTerm, CancellationToken cancellationToken = default);
}
