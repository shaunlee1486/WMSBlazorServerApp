using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Entities.Inbound;

namespace WMS.Infrastructure.Persistence.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(po => po.Id);

        builder.Property(po => po.PONumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(po => po.PONumber).IsUnique();

        builder.Property(po => po.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(po => po.Note).HasMaxLength(1000);

        builder.HasOne(po => po.Supplier)
            .WithMany()
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItems");
        builder.HasKey(poi => poi.Id);

        builder.Property(poi => poi.OrderedQty).HasPrecision(18, 4).IsRequired();
        builder.Property(poi => poi.ReceivedQty).HasPrecision(18, 4).IsRequired();
        builder.Property(poi => poi.UnitPrice).HasPrecision(18, 4).IsRequired();

        builder.HasOne(poi => poi.PurchaseOrder)
            .WithMany(po => po.Items)
            .HasForeignKey(poi => poi.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(poi => poi.Product)
            .WithMany()
            .HasForeignKey(poi => poi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class GoodsReceiptConfiguration : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("GoodsReceipts");
        builder.HasKey(gr => gr.Id);

        builder.Property(gr => gr.GRNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(gr => gr.GRNumber).IsUnique();

        builder.Property(gr => gr.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(gr => gr.Note).HasMaxLength(1000);

        builder.HasOne(gr => gr.PurchaseOrder)
            .WithMany()
            .HasForeignKey(gr => gr.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class GoodsReceiptItemConfiguration : IEntityTypeConfiguration<GoodsReceiptItem>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptItem> builder)
    {
        builder.ToTable("GoodsReceiptItems");
        builder.HasKey(gri => gri.Id);

        builder.Property(gri => gri.ReceivedQty).HasPrecision(18, 4).IsRequired();
        builder.Property(gri => gri.BatchNo).HasMaxLength(100);

        builder.HasOne(gri => gri.GoodsReceipt)
            .WithMany(gr => gr.Items)
            .HasForeignKey(gri => gri.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gri => gri.Product)
            .WithMany()
            .HasForeignKey(gri => gri.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(gri => gri.Location)
            .WithMany()
            .HasForeignKey(gri => gri.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
