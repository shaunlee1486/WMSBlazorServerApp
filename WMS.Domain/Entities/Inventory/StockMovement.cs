using System;
using WMS.Domain.Entities.MasterData;
using WMS.SharedKernel.Enums;

namespace WMS.Domain.Entities.Inventory;

public class StockMovement
{
    public Guid Id { get; set; }
    
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    public Guid? FromLocationId { get; set; }
    public Location? FromLocation { get; set; }
    
    public Guid? ToLocationId { get; set; }
    public Location? ToLocation { get; set; }
    
    public decimal Quantity { get; set; }
    public MovementType MovementType { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Note { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
