using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Entities.Internal;
using WMS.Domain.Entities.Reporting;

namespace WMS.Infrastructure.Persistence.Configurations;

public class TransferOrderConfiguration : IEntityTypeConfiguration<TransferOrder>
{
    public void Configure(EntityTypeBuilder<TransferOrder> builder)
    {
        builder.ToTable("TransferOrders");
        builder.HasKey(to => to.Id);

        builder.Property(to => to.TONumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(to => to.TONumber).IsUnique();

        builder.Property(to => to.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(to => to.FromWarehouse)
            .WithMany()
            .HasForeignKey(to => to.FromWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(to => to.ToWarehouse)
            .WithMany()
            .HasForeignKey(to => to.ToWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TransferOrderItemConfiguration : IEntityTypeConfiguration<TransferOrderItem>
{
    public void Configure(EntityTypeBuilder<TransferOrderItem> builder)
    {
        builder.ToTable("TransferOrderItems");
        builder.HasKey(toi => toi.Id);

        builder.Property(toi => toi.Qty).HasPrecision(18, 4).IsRequired();
        
        builder.Property(toi => toi.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(toi => toi.TransferOrder)
            .WithMany(to => to.Items)
            .HasForeignKey(toi => toi.TransferOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(toi => toi.Product)
            .WithMany()
            .HasForeignKey(toi => toi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(toi => toi.FromLocation)
            .WithMany()
            .HasForeignKey(toi => toi.FromLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(toi => toi.ToLocation)
            .WithMany()
            .HasForeignKey(toi => toi.ToLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReturnConfiguration : IEntityTypeConfiguration<Return>
{
    public void Configure(EntityTypeBuilder<Return> builder)
    {
        builder.ToTable("Returns");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReturnNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(r => r.ReturnNumber).IsUnique();

        builder.Property(r => r.ReturnType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.ReferenceNo).HasMaxLength(100).IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Note).HasMaxLength(1000);
    }
}

public class ReturnItemConfiguration : IEntityTypeConfiguration<ReturnItem>
{
    public void Configure(EntityTypeBuilder<ReturnItem> builder)
    {
        builder.ToTable("ReturnItems");
        builder.HasKey(ri => ri.Id);

        builder.Property(ri => ri.Quantity).HasPrecision(18, 4).IsRequired();

        builder.Property(ri => ri.InspectionStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ri => ri.Note).HasMaxLength(1000);

        builder.HasOne(ri => ri.Return)
            .WithMany(r => r.Items)
            .HasForeignKey(ri => ri.ReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ri => ri.Product)
            .WithMany()
            .HasForeignKey(ri => ri.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ri => ri.Location)
            .WithMany()
            .HasForeignKey(ri => ri.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("AppSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Key).IsUnique();

        builder.Property(x => x.Value).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Group).HasMaxLength(100).IsRequired();
    }
}

