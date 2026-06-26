using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Entities.MasterData;
using WMS.Domain.Entities.Reporting;

namespace WMS.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(w => w.Code).IsUnique();
        builder.Property(w => w.Name).HasMaxLength(200).IsRequired();
    }
}

public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.ToTable("Zones");
        builder.HasKey(z => z.Id);
        builder.Property(z => z.Code).HasMaxLength(50).IsRequired();
        builder.Property(z => z.Name).HasMaxLength(200).IsRequired();
        
        // Map enum to string in PostgreSQL
        builder.Property(z => z.ZoneType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(z => new { z.WarehouseId, z.Code }).IsUnique();

        builder.HasOne(z => z.Warehouse)
            .WithMany(w => w.Zones)
            .HasForeignKey(z => z.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Aisle).HasMaxLength(20);
        builder.Property(l => l.Bay).HasMaxLength(20);
        builder.Property(l => l.Level).HasMaxLength(20);
        builder.Property(l => l.Position).HasMaxLength(20);
        builder.Property(l => l.Barcode).HasMaxLength(100);
        builder.HasIndex(l => l.Barcode).IsUnique();

        builder.HasOne(l => l.Zone)
            .WithMany(z => z.Locations)
            .HasForeignKey(l => l.ZoneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(s => s.Code).IsUnique();
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.ContactPerson).HasMaxLength(100);
        builder.Property(s => s.Phone).HasMaxLength(50);
        builder.Property(s => s.Email).HasMaxLength(100);
    }
}

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.ContactPerson).HasMaxLength(100);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Email).HasMaxLength(100);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();

        // Self-referencing relationship
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Units");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(u => u.Code).IsUnique();
        builder.Property(u => u.Name).HasMaxLength(100).IsRequired();
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Barcode).HasMaxLength(100);
        builder.HasIndex(p => p.Barcode).IsUnique();
        builder.Property(p => p.ImagePath).HasMaxLength(500);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Unit)
            .WithMany()
            .HasForeignKey(p => p.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(50).IsRequired();
    }
}
