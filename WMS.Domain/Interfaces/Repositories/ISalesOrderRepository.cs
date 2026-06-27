using System;
using System.Threading;
using System.Threading.Tasks;
using WMS.Domain.Entities.Outbound;
using WMS.SharedKernel;

namespace WMS.Domain.Interfaces.Repositories;

public interface ISalesOrderRepository : IRepository<SalesOrder>
{
    Task<SalesOrder?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<PagedResult<SalesOrder>> GetSalesOrdersPagedAsync(
        string? searchTerm,
        string? status,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<string> GetNextSONumberAsync(CancellationToken cancellationToken = default);
}
