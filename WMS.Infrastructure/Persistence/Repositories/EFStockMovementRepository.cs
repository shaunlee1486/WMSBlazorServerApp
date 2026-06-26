using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Entities.Inventory;
using WMS.Domain.Interfaces.Repositories;
using WMS.Infrastructure.Persistence.Repositories;
using WMS.SharedKernel;

namespace WMS.Infrastructure.Persistence.Repositories;

public class EFStockMovementRepository : EFRepository<StockMovement>, IStockMovementRepository
{
    public EFStockMovementRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<PagedResult<StockMovement>> GetStockMovementsPagedAsync(
        string? searchTerm,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(sm => sm.Product)
            .Include(sm => sm.FromLocation)
            .Include(sm => sm.ToLocation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(sm => sm.Product.Code.ToLower().Contains(search)
                                   || sm.Product.Name.ToLower().Contains(search)
                                   || (sm.ReferenceNo != null && sm.ReferenceNo.ToLower().Contains(search))
                                   || (sm.FromLocation != null && sm.FromLocation.Barcode.ToLower().Contains(search))
                                   || (sm.ToLocation != null && sm.ToLocation.Barcode.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Dynamic Sorting Map
        if (!string.IsNullOrWhiteSpace(sortColumn))
        {
            query = sortColumn.ToLower() switch
            {
                "productcode" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(sm => sm.Product.Code) : query.OrderBy(sm => sm.Product.Code),
                "quantity" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(sm => sm.Quantity) : query.OrderBy(sm => sm.Quantity),
                "movementtype" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(sm => sm.MovementType) : query.OrderBy(sm => sm.MovementType),
                "referenceno" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(sm => sm.ReferenceNo) : query.OrderBy(sm => sm.ReferenceNo),
                _ => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(sm => sm.CreatedAt) : query.OrderBy(sm => sm.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(sm => sm.CreatedAt);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<StockMovement>(items, totalCount, page, pageSize);
    }
}
