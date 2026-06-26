using System;
using System.Threading;
using System.Threading.Tasks;
using WMS.Domain.Entities.Inbound;
using WMS.SharedKernel;

namespace WMS.Domain.Interfaces.Repositories;

public interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
{
    Task<PurchaseOrder?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<PagedResult<PurchaseOrder>> GetPurchaseOrdersPagedAsync(
        string? searchTerm,
        string? status,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<string> GetNextPONumberAsync(CancellationToken cancellationToken = default);
}
