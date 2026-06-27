using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Entities.Outbound;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Infrastructure.Persistence.Repositories;

public class EFSalesOrderRepository : EFRepository<SalesOrder>, ISalesOrderRepository
{
    public EFSalesOrderRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<SalesOrder?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(so => so.Customer)
            .Include(so => so.Items)
                .ThenInclude(soi => soi.Product)
            .FirstOrDefaultAsync(so => so.Id == id, cancellationToken);
    }

    public async Task<PagedResult<SalesOrder>> GetSalesOrdersPagedAsync(
        string? searchTerm,
        string? status,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(so => so.Customer)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SalesOrderStatus>(status, true, out var statusEnum))
        {
            query = query.Where(so => so.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(so => so.SONumber.ToLower().Contains(search)
                                   || (so.Note != null && so.Note.ToLower().Contains(search))
                                   || so.Customer.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortColumn))
        {
            query = sortColumn.ToLower() switch
            {
                "sonumber" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(so => so.SONumber) : query.OrderBy(so => so.SONumber),
                "customername" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(so => so.Customer.Name) : query.OrderBy(so => so.Customer.Name),
                "orderdate" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(so => so.OrderDate) : query.OrderBy(so => so.OrderDate),
                "status" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(so => so.Status) : query.OrderBy(so => so.Status),
                _ => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(so => so.CreatedAt) : query.OrderBy(so => so.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(so => so.CreatedAt);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<SalesOrder>(items, totalCount, page, pageSize);
    }

    public async Task<string> GetNextSONumberAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var prefix = $"SO-{today:yyyyMM}-";

        var lastSONumber = await DbSet
            .IgnoreQueryFilters()
            .Where(so => so.SONumber.StartsWith(prefix))
            .OrderByDescending(so => so.SONumber)
            .Select(so => so.SONumber)
            .FirstOrDefaultAsync(cancellationToken);

        int sequence = 1;
        if (lastSONumber != null && lastSONumber.Length > prefix.Length)
        {
            var suffix = lastSONumber.Substring(prefix.Length);
            if (int.TryParse(suffix, out int parsedSeq))
            {
                sequence = parsedSeq + 1;
            }
        }

        return $"{prefix}{sequence:D5}";
    }
}
