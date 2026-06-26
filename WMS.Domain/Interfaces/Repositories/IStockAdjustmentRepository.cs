using System;
using System.Threading;
using System.Threading.Tasks;
using WMS.Domain.Entities.Inventory;
using WMS.SharedKernel;

namespace WMS.Domain.Interfaces.Repositories;

public interface IStockAdjustmentRepository : IRepository<StockAdjustment>
{
    Task<StockAdjustment?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<PagedResult<StockAdjustment>> GetStockAdjustmentsPagedAsync(
        string? searchTerm,
        Guid? warehouseId,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<string> GetNextAdjustmentNumberAsync(CancellationToken cancellationToken = default);
}
