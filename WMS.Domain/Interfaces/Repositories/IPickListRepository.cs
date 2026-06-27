using System;
using System.Threading;
using System.Threading.Tasks;
using WMS.Domain.Entities.Outbound;
using WMS.SharedKernel;

namespace WMS.Domain.Interfaces.Repositories;

public interface IPickListRepository : IRepository<PickList>
{
    Task<PickList?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<PagedResult<PickList>> GetPickListsPagedAsync(
        string? searchTerm,
        string? status,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<string> GetNextPickListNumberAsync(CancellationToken cancellationToken = default);
}
