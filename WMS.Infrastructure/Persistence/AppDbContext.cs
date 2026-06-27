using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Entities.Identity;
using WMS.Domain.Entities.Inbound;
using WMS.Domain.Entities.Outbound;
using WMS.Domain.Entities.Inventory;
using WMS.Domain.Entities.MasterData;
using WMS.Domain.Entities.Reporting;
using WMS.Domain.Interfaces;
using WMS.SharedKernel;

namespace WMS.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IUnitOfWork
{
    private readonly ICurrentUserService _currentUserService;
    private IDbContextTransaction? _currentTransaction;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    // Master Data
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Product> Products => Set<Product>();

    // Reporting / System
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Inventory
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<StockAdjustmentItem> StockAdjustmentItems => Set<StockAdjustmentItem>();

    // Inbound
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptItem> GoodsReceiptItems => Set<GoodsReceiptItem>();

    // Outbound
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderItem> SalesOrderItems => Set<SalesOrderItem>();
    public DbSet<PickList> PickLists => Set<PickList>();
    public DbSet<PickListItem> PickListItems => Set<PickListItem>();
    public DbSet<GoodsIssue> GoodsIssues => Set<GoodsIssue>();
    public DbSet<GoodsIssueItem> GoodsIssueItems => Set<GoodsIssueItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Rename Identity Tables
        builder.Entity<AppUser>().ToTable("Users");
        builder.Entity<AppRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        // Apply EntityConfigurations
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply Soft-delete filters
        builder.Entity<Warehouse>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Zone>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Location>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Supplier>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Customer>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Category>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Unit>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Product>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<StockAdjustment>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<PurchaseOrder>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<GoodsReceipt>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<SalesOrder>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<PickList>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<GoodsIssue>().HasQueryFilter(x => !x.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Track timestamps and handle soft-deletes
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    // Intercept hard delete and convert to soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        // Generate Audit Logs
        var auditEntries = OnBeforeSaveChanges();
        
        var result = await base.SaveChangesAsync(cancellationToken);
        
        await OnAfterSaveChanges(auditEntries);
        
        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();
        var userId = _currentUserService.UserId;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                EntityName = entry.Entity.GetType().Name,
                UserId = userId,
                Action = entry.State switch
                {
                    EntityState.Added => "Create",
                    EntityState.Deleted => "Delete",
                    EntityState.Modified => "Update",
                    _ => "Unknown"
                }
            };
            
            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue!;
                    continue;
                }

                if (property.IsTemporary)
                {
                    auditEntry.TemporaryProperties.Add(property);
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = property.CurrentValue!;
                        break;
                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = property.OriginalValue!;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.OldValues[propertyName] = property.OriginalValue!;
                            auditEntry.NewValues[propertyName] = property.CurrentValue!;
                        }
                        break;
                }
            }
        }

        foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
        {
            AuditLogs.Add(auditEntry.ToAuditLog());
        }

        return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
    }

    private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return Task.CompletedTask;

        foreach (var auditEntry in auditEntries)
        {
            foreach (var prop in auditEntry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue!;
                }
                else
                {
                    auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue!;
                }
            }

            AuditLogs.Add(auditEntry.ToAuditLog());
        }

        return base.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null) return;
        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    // Nested helper class for change tracking audits
    private class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public string EntityName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new();
        public Dictionary<string, object> OldValues { get; } = new();
        public Dictionary<string, object> NewValues { get; } = new();
        public List<PropertyEntry> TemporaryProperties { get; } = new();

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public AuditLog ToAuditLog()
        {
            return new AuditLog
            {
                Id = Guid.CreateVersion7(),
                EntityName = EntityName,
                EntityId = KeyValues.Count == 1 ? KeyValues.Values.First().ToString()! : JsonSerializer.Serialize(KeyValues),
                Action = Action,
                UserId = UserId,
                Timestamp = DateTime.UtcNow,
                OldValues = OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues),
                NewValues = NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues)
            };
        }
    }
}
