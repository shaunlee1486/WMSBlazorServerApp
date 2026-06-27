using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Entities.Outbound;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Infrastructure.Persistence.Repositories;

public class EFGoodsIssueRepository : EFRepository<GoodsIssue>, IGoodsIssueRepository
{
    public EFGoodsIssueRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<GoodsIssue?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(gi => gi.SalesOrder)
                .ThenInclude(so => so.Customer)
            .Include(gi => gi.Items)
                .ThenInclude(gii => gii.Product)
            .Include(gi => gi.Items)
                .ThenInclude(gii => gii.Location)
            .FirstOrDefaultAsync(gi => gi.Id == id, cancellationToken);
    }

    public async Task<PagedResult<GoodsIssue>> GetGoodsIssuesPagedAsync(
        string? searchTerm,
        string? status,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(gi => gi.SalesOrder)
                .ThenInclude(so => so.Customer)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<GoodsIssueStatus>(status, true, out var statusEnum))
        {
            query = query.Where(gi => gi.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(gi => gi.GINumber.ToLower().Contains(search)
                                   || gi.SalesOrder.SONumber.ToLower().Contains(search)
                                   || gi.SalesOrder.Customer.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortColumn))
        {
            query = sortColumn.ToLower() switch
            {
                "ginumber" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(gi => gi.GINumber) : query.OrderBy(gi => gi.GINumber),
                "sonumber" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(gi => gi.SalesOrder.SONumber) : query.OrderBy(gi => gi.SalesOrder.SONumber),
                "customername" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(gi => gi.SalesOrder.Customer.Name) : query.OrderBy(gi => gi.SalesOrder.Customer.Name),
                "status" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(gi => gi.Status) : query.OrderBy(gi => gi.Status),
                _ => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(gi => gi.CreatedAt) : query.OrderBy(gi => gi.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(gi => gi.CreatedAt);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<GoodsIssue>(items, totalCount, page, pageSize);
    }

    public async Task<string> GetNextGINumberAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var prefix = $"GI-{today:yyyyMM}-";

        var lastGINumber = await DbSet
            .IgnoreQueryFilters()
            .Where(gi => gi.GINumber.StartsWith(prefix))
            .OrderByDescending(gi => gi.GINumber)
            .Select(gi => gi.GINumber)
            .FirstOrDefaultAsync(cancellationToken);

        int sequence = 1;
        if (lastGINumber != null && lastGINumber.Length > prefix.Length)
        {
            var suffix = lastGINumber.Substring(prefix.Length);
            if (int.TryParse(suffix, out int parsedSeq))
            {
                sequence = parsedSeq + 1;
            }
        }

        return $"{prefix}{sequence:D5}";
    }

    public async Task<IReadOnlyList<GoodsIssueItem>> GetOutboundReportAsync(Guid? customerId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var query = DbContext.Set<GoodsIssueItem>().AsNoTracking()
            .Include(gii => gii.GoodsIssue)
                .ThenInclude(gi => gi.SalesOrder)
                    .ThenInclude(so => so.Customer)
            .Include(gii => gii.Product)
            .Include(gii => gii.Location)
            .AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(gii => gii.GoodsIssue.SalesOrder.CustomerId == customerId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(gii => gii.GoodsIssue.IssuedDate >= startDate.Value.ToUniversalTime());
        }

        if (endDate.HasValue)
        {
            query = query.Where(gii => gii.GoodsIssue.IssuedDate <= endDate.Value.ToUniversalTime());
        }

        return await query.OrderByDescending(gii => gii.GoodsIssue.IssuedDate).ToListAsync(cancellationToken);
    }
}
