using System;
using System.Threading;
using System.Threading.Tasks;
using WMS.Domain.Entities.Outbound;
using WMS.SharedKernel;

namespace WMS.Domain.Interfaces.Repositories;

public interface IGoodsIssueRepository : IRepository<GoodsIssue>
{
    Task<GoodsIssue?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<PagedResult<GoodsIssue>> GetGoodsIssuesPagedAsync(
        string? searchTerm,
        string? status,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<string> GetNextGINumberAsync(CancellationToken cancellationToken = default);
}
