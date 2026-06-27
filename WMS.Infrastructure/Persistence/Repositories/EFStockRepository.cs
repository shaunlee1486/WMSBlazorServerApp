using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Entities.Inventory;
using WMS.Domain.Entities.Reporting;
using WMS.Domain.Interfaces.Repositories;
using WMS.Infrastructure.Persistence.Repositories;
using WMS.SharedKernel;

namespace WMS.Infrastructure.Persistence.Repositories;

public class EFStockRepository : EFRepository<Stock>, IStockRepository
{
    public EFStockRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<PagedResult<Stock>> GetStockOverviewPagedAsync(
        string? searchTerm,
        Guid? warehouseId,
        string? sortColumn,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(s => s.Product)
            .Include(s => s.Location)
                .ThenInclude(l => l.Zone)
                    .ThenInclude(z => z.Warehouse)
            .AsQueryable();

        // Apply filters
        if (warehouseId.HasValue)
        {
            query = query.Where(s => s.Location.Zone.WarehouseId == warehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(s => s.Product.Code.ToLower().Contains(search)
                                  || s.Product.Name.ToLower().Contains(search)
                                  || s.Location!.Barcode!.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortColumn))
        {
            // Simple mapping for sorting columns
            query = sortColumn.ToLower() switch
            {
                "productcode" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(s => s.Product.Code) : query.OrderBy(s => s.Product.Code),
                "productname" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(s => s.Product.Name) : query.OrderBy(s => s.Product.Name),
                "locationbarcode" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(s => s.Location.Barcode) : query.OrderBy(s => s.Location.Barcode),
                "quantity" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(s => s.Quantity) : query.OrderBy(s => s.Quantity),
                "reservedquantity" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(s => s.ReservedQuantity) : query.OrderBy(s => s.ReservedQuantity),
                "availablequantity" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(s => s.Quantity - s.ReservedQuantity) : query.OrderBy(s => s.Quantity - s.ReservedQuantity),
                _ => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(s => s.LastUpdatedAt) : query.OrderBy(s => s.LastUpdatedAt)
            };
        }
        else
        {
            query = query.OrderBy(s => s.Product.Code);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Stock>(items, totalCount, page, pageSize);
    }

    public async Task<IReadOnlyList<Stock>> GetLowStockReportAsync(CancellationToken cancellationToken = default)
    {
        // Low Stock condition: Current Stock <= Product.ReorderPoint
        return await DbSet.AsNoTracking()
            .Include(s => s.Product)
                .ThenInclude(p => p.Category)
            .Include(s => s.Location)
                .ThenInclude(l => l.Zone)
                    .ThenInclude(z => z.Warehouse)
            .Where(s => s.Quantity <= s.Product.ReorderPoint)
            .OrderBy(s => s.Product.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetLowStockCountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .CountAsync(s => s.Quantity <= s.Product.ReorderPoint, cancellationToken);
    }

    public async Task<IReadOnlyList<StockCategoryStats>> GetStockCategoryStatsAsync(CancellationToken cancellationToken = default)
    {
        // 1. Fetch all stock grouped by product and category
        var stockGroups = await DbSet.AsNoTracking()
            .Include(s => s.Product)
                .ThenInclude(p => p.Category)
            .GroupBy(s => new { CategoryName = s.Product.Category.Name, ProductId = s.ProductId })
            .Select(g => new
            {
                CategoryName = g.Key.CategoryName,
                ProductId = g.Key.ProductId,
                TotalQty = g.Sum(s => s.Quantity)
            })
            .ToListAsync(cancellationToken);

        // 2. Fetch average purchase price per product
        var avgPrices = await DbContext.PurchaseOrderItems.AsNoTracking()
            .GroupBy(poi => poi.ProductId)
            .Select(g => new { ProductId = g.Key, AvgPrice = g.Average(poi => poi.UnitPrice) })
            .ToDictionaryAsync(x => x.ProductId, x => x.AvgPrice, cancellationToken);

        // 3. Group by category and sum up quantity and value
        var categoryStats = stockGroups
            .GroupBy(sg => sg.CategoryName)
            .Select(g => {
                decimal totalVal = 0;
                decimal totalQty = 0;
                foreach (var item in g)
                {
                    totalQty += item.TotalQty;
                    decimal price = avgPrices.TryGetValue(item.ProductId, out var p) ? p : 15.00m;
                    totalVal += item.TotalQty * price;
                }
                return new StockCategoryStats
                {
                    CategoryName = g.Key,
                    TotalQuantity = totalQty,
                    TotalValue = totalVal
                };
            })
            .ToList();

        return categoryStats;
    }

    public async Task<IReadOnlyList<Stock>> GetStockSnapshotReportAsync(Guid? warehouseId, Guid? categoryId, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(s => s.Product)
                .ThenInclude(p => p.Category)
            .Include(s => s.Location)
                .ThenInclude(l => l.Zone)
                    .ThenInclude(z => z.Warehouse)
            .AsQueryable();

        if (warehouseId.HasValue)
        {
            query = query.Where(s => s.Location.Zone.WarehouseId == warehouseId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(s => s.Product.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(s => s.Product.Code.ToLower().Contains(search)
                                  || s.Product.Name.ToLower().Contains(search)
                                  || s.Location!.Barcode!.ToLower().Contains(search));
        }

        return await query.OrderBy(s => s.Product.Code).ToListAsync(cancellationToken);
    }
}
