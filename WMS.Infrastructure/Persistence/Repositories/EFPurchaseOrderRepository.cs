using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Entities.Inbound;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Infrastructure.Persistence.Repositories;

public class EFPurchaseOrderRepository : EFRepository<PurchaseOrder>, IPurchaseOrderRepository
{
    public EFPurchaseOrderRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<PurchaseOrder?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(po => po.Supplier)
            .Include(po => po.Items)
                .ThenInclude(poi => poi.Product)
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);
    }

    public async Task<PagedResult<PurchaseOrder>> GetPurchaseOrdersPagedAsync(
        string? searchTerm,
        string? status,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(po => po.Supplier)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PurchaseOrderStatus>(status, true, out var statusEnum))
        {
            query = query.Where(po => po.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(po => po.PONumber.ToLower().Contains(search)
                                   || (po.Note != null && po.Note.ToLower().Contains(search))
                                   || po.Supplier.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortColumn))
        {
            query = sortColumn.ToLower() switch
            {
                "ponumber" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(po => po.PONumber) : query.OrderBy(po => po.PONumber),
                "suppliername" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(po => po.Supplier.Name) : query.OrderBy(po => po.Supplier.Name),
                "orderdate" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(po => po.OrderDate) : query.OrderBy(po => po.OrderDate),
                "status" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(po => po.Status) : query.OrderBy(po => po.Status),
                _ => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(po => po.CreatedAt) : query.OrderBy(po => po.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(po => po.CreatedAt);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PurchaseOrder>(items, totalCount, page, pageSize);
    }

    public async Task<string> GetNextPONumberAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var prefix = $"PO-{today:yyyyMM}-";

        var lastPONumber = await DbSet
            .IgnoreQueryFilters()
            .Where(po => po.PONumber.StartsWith(prefix))
            .OrderByDescending(po => po.PONumber)
            .Select(po => po.PONumber)
            .FirstOrDefaultAsync(cancellationToken);

        int sequence = 1;
        if (lastPONumber != null && lastPONumber.Length > prefix.Length)
        {
            var suffix = lastPONumber.Substring(prefix.Length);
            if (int.TryParse(suffix, out int parsedSeq))
            {
                sequence = parsedSeq + 1;
            }
        }

        return $"{prefix}{sequence:D5}";
    }
}
