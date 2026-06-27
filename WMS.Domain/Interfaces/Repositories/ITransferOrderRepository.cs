using System;
using System.Threading;
using System.Threading.Tasks;
using WMS.Domain.Entities.Internal;
using WMS.SharedKernel;

namespace WMS.Domain.Interfaces.Repositories;

public interface ITransferOrderRepository : IRepository<TransferOrder>
{
    Task<TransferOrder?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<PagedResult<TransferOrder>> GetTransferOrdersPagedAsync(
        string? searchTerm,
        string? status,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<string> GetNextTONumberAsync(CancellationToken cancellationToken = default);
}
