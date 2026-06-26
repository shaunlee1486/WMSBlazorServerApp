using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Entities.Inventory;

namespace WMS.Infrastructure.Persistence.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable("Stock");
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Quantity).HasPrecision(18, 4).IsRequired();
        builder.Property(s => s.ReservedQuantity).HasPrecision(18, 4).IsRequired();

        builder.HasIndex(s => new { s.ProductId, s.LocationId }).IsUnique();

        builder.HasOne(s => s.Product)
            .WithMany()
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Location)
            .WithMany()
            .HasForeignKey(s => s.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.Quantity).HasPrecision(18, 4).IsRequired();
        
        builder.Property(sm => sm.MovementType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(sm => sm.ReferenceNo).HasMaxLength(100);

        builder.HasOne(sm => sm.Product)
            .WithMany()
            .HasForeignKey(sm => sm.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sm => sm.FromLocation)
            .WithMany()
            .HasForeignKey(sm => sm.FromLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(sm => sm.ToLocation)
            .WithMany()
            .HasForeignKey(sm => sm.ToLocationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ToTable("StockAdjustments");
        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.AdjNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(sa => sa.AdjNumber).IsUnique();

        builder.Property(sa => sa.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(sa => sa.Warehouse)
            .WithMany()
            .HasForeignKey(sa => sa.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StockAdjustmentItemConfiguration : IEntityTypeConfiguration<StockAdjustmentItem>
{
    public void Configure(EntityTypeBuilder<StockAdjustmentItem> builder)
    {
        builder.ToTable("StockAdjustmentItems");
        builder.HasKey(sai => sai.Id);

        builder.Property(sai => sai.SystemQty).HasPrecision(18, 4).IsRequired();
        builder.Property(sai => sai.ActualQty).HasPrecision(18, 4).IsRequired();
        builder.Property(sai => sai.Difference).HasPrecision(18, 4).IsRequired();

        builder.HasOne(sai => sai.StockAdjustment)
            .WithMany(sa => sa.Items)
            .HasForeignKey(sai => sai.StockAdjustmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sai => sai.Product)
            .WithMany()
            .HasForeignKey(sai => sai.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sai => sai.Location)
            .WithMany()
            .HasForeignKey(sai => sai.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
