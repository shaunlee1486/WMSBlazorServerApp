using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Entities.Outbound;

namespace WMS.Infrastructure.Persistence.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");
        builder.HasKey(so => so.Id);

        builder.Property(so => so.SONumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(so => so.SONumber).IsUnique();

        builder.Property(so => so.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(so => so.Note).HasMaxLength(1000);

        builder.HasOne(so => so.Customer)
            .WithMany()
            .HasForeignKey(so => so.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
{
    public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
    {
        builder.ToTable("SalesOrderItems");
        builder.HasKey(soi => soi.Id);

        builder.Property(soi => soi.OrderedQty).HasPrecision(18, 4).IsRequired();
        builder.Property(soi => soi.PickedQty).HasPrecision(18, 4).IsRequired();
        builder.Property(soi => soi.UnitPrice).HasPrecision(18, 4).IsRequired();

        builder.HasOne(soi => soi.SalesOrder)
            .WithMany(so => so.Items)
            .HasForeignKey(soi => soi.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(soi => soi.Product)
            .WithMany()
            .HasForeignKey(soi => soi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PickListConfiguration : IEntityTypeConfiguration<PickList>
{
    public void Configure(EntityTypeBuilder<PickList> builder)
    {
        builder.ToTable("PickLists");
        builder.HasKey(pl => pl.Id);

        builder.Property(pl => pl.PickListNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(pl => pl.PickListNumber).IsUnique();

        builder.Property(pl => pl.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(pl => pl.SalesOrder)
            .WithMany()
            .HasForeignKey(pl => pl.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PickListItemConfiguration : IEntityTypeConfiguration<PickListItem>
{
    public void Configure(EntityTypeBuilder<PickListItem> builder)
    {
        builder.ToTable("PickListItems");
        builder.HasKey(pli => pli.Id);

        builder.Property(pli => pli.RequiredQty).HasPrecision(18, 4).IsRequired();
        builder.Property(pli => pli.PickedQty).HasPrecision(18, 4).IsRequired();
        builder.Property(pli => pli.Status).HasMaxLength(50).IsRequired();

        builder.HasOne(pli => pli.PickList)
            .WithMany(pl => pl.Items)
            .HasForeignKey(pli => pli.PickListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pli => pli.Product)
            .WithMany()
            .HasForeignKey(pli => pli.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pli => pli.Location)
            .WithMany()
            .HasForeignKey(pli => pli.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class GoodsIssueConfiguration : IEntityTypeConfiguration<GoodsIssue>
{
    public void Configure(EntityTypeBuilder<GoodsIssue> builder)
    {
        builder.ToTable("GoodsIssues");
        builder.HasKey(gi => gi.Id);

        builder.Property(gi => gi.GINumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(gi => gi.GINumber).IsUnique();

        builder.Property(gi => gi.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(gi => gi.Note).HasMaxLength(1000);

        builder.HasOne(gi => gi.SalesOrder)
            .WithMany()
            .HasForeignKey(gi => gi.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class GoodsIssueItemConfiguration : IEntityTypeConfiguration<GoodsIssueItem>
{
    public void Configure(EntityTypeBuilder<GoodsIssueItem> builder)
    {
        builder.ToTable("GoodsIssueItems");
        builder.HasKey(gii => gii.Id);

        builder.Property(gii => gii.IssuedQty).HasPrecision(18, 4).IsRequired();
        builder.Property(gii => gii.BatchNo).HasMaxLength(100);

        builder.HasOne(gii => gii.GoodsIssue)
            .WithMany(gi => gi.Items)
            .HasForeignKey(gii => gii.GoodsIssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gii => gii.Product)
            .WithMany()
            .HasForeignKey(gii => gii.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(gii => gii.Location)
            .WithMany()
            .HasForeignKey(gii => gii.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
