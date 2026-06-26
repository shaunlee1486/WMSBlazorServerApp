using System;
using System.Threading;
using System.Threading.Tasks;
using WMS.Domain.Entities.Inventory;
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
}
