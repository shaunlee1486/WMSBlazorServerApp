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

public class EFPickListRepository : EFRepository<PickList>, IPickListRepository
{
    public EFPickListRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<PickList?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(pl => pl.SalesOrder)
                .ThenInclude(so => so.Customer)
            .Include(pl => pl.Items)
                .ThenInclude(pli => pli.Product)
            .Include(pl => pl.Items)
                .ThenInclude(pli => pli.Location)
            .FirstOrDefaultAsync(pl => pl.Id == id, cancellationToken);
    }

    public async Task<PagedResult<PickList>> GetPickListsPagedAsync(
        string? searchTerm,
        string? status,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(pl => pl.SalesOrder)
                .ThenInclude(so => so.Customer)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PickListStatus>(status, true, out var statusEnum))
        {
            query = query.Where(pl => pl.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(pl => pl.PickListNumber.ToLower().Contains(search)
                                   || pl.SalesOrder.SONumber.ToLower().Contains(search)
                                   || pl.SalesOrder.Customer.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortColumn))
        {
            query = sortColumn.ToLower() switch
            {
                "picklistnumber" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(pl => pl.PickListNumber) : query.OrderBy(pl => pl.PickListNumber),
                "sonumber" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(pl => pl.SalesOrder.SONumber) : query.OrderBy(pl => pl.SalesOrder.SONumber),
                "customername" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(pl => pl.SalesOrder.Customer.Name) : query.OrderBy(pl => pl.SalesOrder.Customer.Name),
                "status" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(pl => pl.Status) : query.OrderBy(pl => pl.Status),
                _ => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(pl => pl.CreatedAt) : query.OrderBy(pl => pl.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(pl => pl.CreatedAt);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PickList>(items, totalCount, page, pageSize);
    }

    public async Task<string> GetNextPickListNumberAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var prefix = $"PL-{today:yyyyMM}-";

        var lastPLNumber = await DbSet
            .IgnoreQueryFilters()
            .Where(pl => pl.PickListNumber.StartsWith(prefix))
            .OrderByDescending(pl => pl.PickListNumber)
            .Select(pl => pl.PickListNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int sequence = 1;
        if (lastPLNumber != null && lastPLNumber.Length > prefix.Length)
        {
            var suffix = lastPLNumber.Substring(prefix.Length);
            if (int.TryParse(suffix, out int parsedSeq))
            {
                sequence = parsedSeq + 1;
            }
        }

        return $"{prefix}{sequence:D5}";
    }
}
