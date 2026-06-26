using System;
using WMS.Domain.Entities.MasterData;

namespace WMS.Domain.Entities.Inventory;

public class Stock
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    // Calculated helper property
    public decimal AvailableQuantity => Quantity - ReservedQuantity;
}
