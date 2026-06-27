using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Entities.Internal;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Infrastructure.Persistence.Repositories;

public class EFReturnRepository : EFRepository<Return>, IReturnRepository
{
    public EFReturnRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Return?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Items)
                .ThenInclude(ri => ri.Product)
            .Include(r => r.Items)
                .ThenInclude(ri => ri.Location)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Return>> GetReturnsPagedAsync(
        string? searchTerm,
        string? status,
        string? returnType,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReturnStatus>(status, true, out var statusEnum))
        {
            query = query.Where(r => r.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(returnType) && Enum.TryParse<ReturnType>(returnType, true, out var typeEnum))
        {
            query = query.Where(r => r.ReturnType == typeEnum);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(r => r.ReturnNumber.ToLower().Contains(search)
                                   || r.ReferenceNo.ToLower().Contains(search)
                                   || (r.Note != null && r.Note.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(sortColumn))
        {
            query = sortColumn.ToLower() switch
            {
                "returnnumber" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(r => r.ReturnNumber) : query.OrderBy(r => r.ReturnNumber),
                "returntype" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(r => r.ReturnType) : query.OrderBy(r => r.ReturnType),
                "referenceno" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(r => r.ReferenceNo) : query.OrderBy(r => r.ReferenceNo),
                "status" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
                _ => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(r => r.CreatedAt);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Return>(items, totalCount, page, pageSize);
    }

    public async Task<string> GetNextReturnNumberAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var prefix = $"RET-{today:yyyyMM}-";

        var lastReturnNumber = await DbSet
            .IgnoreQueryFilters()
            .Where(r => r.ReturnNumber.StartsWith(prefix))
            .OrderByDescending(r => r.ReturnNumber)
            .Select(r => r.ReturnNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int sequence = 1;
        if (lastReturnNumber != null && lastReturnNumber.Length > prefix.Length)
        {
            var suffix = lastReturnNumber.Substring(prefix.Length);
            if (int.TryParse(suffix, out int parsedSeq))
            {
                sequence = parsedSeq + 1;
            }
        }

        return $"{prefix}{sequence:D5}";
    }
}
