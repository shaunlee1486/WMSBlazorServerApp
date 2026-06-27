using System;
using System.Threading;
using System.Threading.Tasks;
using WMS.Domain.Entities.Internal;
using WMS.SharedKernel;

namespace WMS.Domain.Interfaces.Repositories;

public interface IReturnRepository : IRepository<Return>
{
    Task<Return?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<PagedResult<Return>> GetReturnsPagedAsync(
        string? searchTerm,
        string? status,
        string? returnType,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<string> GetNextReturnNumberAsync(CancellationToken cancellationToken = default);
}
