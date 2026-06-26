using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Entities.Inventory;
using WMS.Domain.Interfaces.Repositories;
using WMS.SharedKernel;

namespace WMS.Infrastructure.Persistence.Repositories;

public class EFStockAdjustmentRepository : EFRepository<StockAdjustment>, IStockAdjustmentRepository
{
    public EFStockAdjustmentRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<StockAdjustment?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(sa => sa.Warehouse)
            .Include(sa => sa.Items)
                .ThenInclude(sai => sai.Product)
            .Include(sa => sa.Items)
                .ThenInclude(sai => sai.Location)
            .FirstOrDefaultAsync(sa => sa.Id == id, cancellationToken);
    }

    public async Task<PagedResult<StockAdjustment>> GetStockAdjustmentsPagedAsync(
        string? searchTerm,
        Guid? warehouseId,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(sa => sa.Warehouse)
            .AsQueryable();

        // Apply filters
        if (warehouseId.HasValue)
        {
            query = query.Where(sa => sa.WarehouseId == warehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(sa => sa.AdjNumber.ToLower().Contains(search)
                                   || (sa.Reason != null && sa.Reason.ToLower().Contains(search))
                                   || sa.Warehouse.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortColumn))
        {
            query = sortColumn.ToLower() switch
            {
                "adjnumber" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(sa => sa.AdjNumber) : query.OrderBy(sa => sa.AdjNumber),
                "adjustmentdate" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(sa => sa.AdjustmentDate) : query.OrderBy(sa => sa.AdjustmentDate),
                "warehousename" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(sa => sa.Warehouse.Name) : query.OrderBy(sa => sa.Warehouse.Name),
                "status" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(sa => sa.Status) : query.OrderBy(sa => sa.Status),
                _ => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(sa => sa.CreatedAt) : query.OrderBy(sa => sa.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(sa => sa.CreatedAt);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<StockAdjustment>(items, totalCount, page, pageSize);
    }

    public async Task<string> GetNextAdjustmentNumberAsync(CancellationToken cancellationToken = default)
    {
        var todayStr = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"ADJ-{todayStr}-";

        // Query the database for the last number generated today.
        // Ignore the soft delete query filter so that we don't reuse numbers even if the adjustment was soft-deleted.
        var lastAdjNumber = await DbSet
            .IgnoreQueryFilters()
            .Where(sa => sa.AdjNumber.StartsWith(prefix))
            .OrderByDescending(sa => sa.AdjNumber)
            .Select(sa => sa.AdjNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int sequence = 1;
        if (lastAdjNumber != null && lastAdjNumber.Length > prefix.Length)
        {
            var suffix = lastAdjNumber.Substring(prefix.Length);
            if (int.TryParse(suffix, out int parsedSeq))
            {
                sequence = parsedSeq + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }
}
