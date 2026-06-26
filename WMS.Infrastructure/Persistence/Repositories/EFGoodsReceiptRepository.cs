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

public class EFGoodsReceiptRepository : EFRepository<GoodsReceipt>, IGoodsReceiptRepository
{
    public EFGoodsReceiptRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<GoodsReceipt?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(gr => gr.PurchaseOrder)
                .ThenInclude(po => po.Supplier)
            .Include(gr => gr.Items)
                .ThenInclude(gri => gri.Product)
            .Include(gr => gr.Items)
                .ThenInclude(gri => gri.Location)
            .FirstOrDefaultAsync(gr => gr.Id == id, cancellationToken);
    }

    public async Task<PagedResult<GoodsReceipt>> GetGoodsReceiptsPagedAsync(
        string? searchTerm,
        string? status,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(gr => gr.PurchaseOrder)
                .ThenInclude(po => po.Supplier)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<GoodsReceiptStatus>(status, true, out var statusEnum))
        {
            query = query.Where(gr => gr.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(gr => gr.GRNumber.ToLower().Contains(search)
                                   || gr.PurchaseOrder.PONumber.ToLower().Contains(search)
                                   || (gr.Note != null && gr.Note.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortColumn))
        {
            query = sortColumn.ToLower() switch
            {
                "grnumber" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(gr => gr.GRNumber) : query.OrderBy(gr => gr.GRNumber),
                "ponumber" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(gr => gr.PurchaseOrder.PONumber) : query.OrderBy(gr => gr.PurchaseOrder.PONumber),
                "receiveddate" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(gr => gr.ReceivedDate) : query.OrderBy(gr => gr.ReceivedDate),
                "status" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(gr => gr.Status) : query.OrderBy(gr => gr.Status),
                _ => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(gr => gr.CreatedAt) : query.OrderBy(gr => gr.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(gr => gr.CreatedAt);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<GoodsReceipt>(items, totalCount, page, pageSize);
    }

    public async Task<string> GetNextGRNumberAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var prefix = $"GR-{today:yyyyMM}-";

        var lastGRNumber = await DbSet
            .IgnoreQueryFilters()
            .Where(gr => gr.GRNumber.StartsWith(prefix))
            .OrderByDescending(gr => gr.GRNumber)
            .Select(gr => gr.GRNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int sequence = 1;
        if (lastGRNumber != null && lastGRNumber.Length > prefix.Length)
        {
            var suffix = lastGRNumber.Substring(prefix.Length);
            if (int.TryParse(suffix, out int parsedSeq))
            {
                sequence = parsedSeq + 1;
            }
        }

        return $"{prefix}{sequence:D5}";
    }
}
