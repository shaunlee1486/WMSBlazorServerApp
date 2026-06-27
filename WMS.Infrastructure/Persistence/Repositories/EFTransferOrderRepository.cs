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

public class EFTransferOrderRepository : EFRepository<TransferOrder>, ITransferOrderRepository
{
    public EFTransferOrderRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<TransferOrder?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(to => to.FromWarehouse)
            .Include(to => to.ToWarehouse)
            .Include(to => to.Items)
                .ThenInclude(toi => toi.Product)
            .Include(to => to.Items)
                .ThenInclude(toi => toi.FromLocation)
            .Include(to => to.Items)
                .ThenInclude(toi => toi.ToLocation)
            .FirstOrDefaultAsync(to => to.Id == id, cancellationToken);
    }

    public async Task<PagedResult<TransferOrder>> GetTransferOrdersPagedAsync(
        string? searchTerm,
        string? status,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(to => to.FromWarehouse)
            .Include(to => to.ToWarehouse)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TransferOrderStatus>(status, true, out var statusEnum))
        {
            query = query.Where(to => to.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(to => to.TONumber.ToLower().Contains(search)
                                   || to.FromWarehouse.Name.ToLower().Contains(search)
                                   || to.ToWarehouse.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(sortColumn))
        {
            query = sortColumn.ToLower() switch
            {
                "tonumber" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(to => to.TONumber) : query.OrderBy(to => to.TONumber),
                "fromwarehouse" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(to => to.FromWarehouse.Name) : query.OrderBy(to => to.FromWarehouse.Name),
                "towarehouse" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(to => to.ToWarehouse.Name) : query.OrderBy(to => to.ToWarehouse.Name),
                "status" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(to => to.Status) : query.OrderBy(to => to.Status),
                _ => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(to => to.CreatedAt) : query.OrderBy(to => to.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(to => to.CreatedAt);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TransferOrder>(items, totalCount, page, pageSize);
    }

    public async Task<string> GetNextTONumberAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var prefix = $"TO-{today:yyyyMM}-";

        var lastTONumber = await DbSet
            .IgnoreQueryFilters()
            .Where(to => to.TONumber.StartsWith(prefix))
            .OrderByDescending(to => to.TONumber)
            .Select(to => to.TONumber)
            .FirstOrDefaultAsync(cancellationToken);

        int sequence = 1;
        if (lastTONumber != null && lastTONumber.Length > prefix.Length)
        {
            var suffix = lastTONumber.Substring(prefix.Length);
            if (int.TryParse(suffix, out int parsedSeq))
            {
                sequence = parsedSeq + 1;
            }
        }

        return $"{prefix}{sequence:D5}";
    }
}
