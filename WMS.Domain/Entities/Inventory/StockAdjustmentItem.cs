using System;
using WMS.Domain.Entities.MasterData;

namespace WMS.Domain.Entities.Inventory;

public class StockAdjustmentItem
{
    public Guid Id { get; set; }
    
    public Guid StockAdjustmentId { get; set; }
    public StockAdjustment StockAdjustment { get; set; } = null!;
    
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;
    
    public decimal SystemQty { get; set; }
    public decimal ActualQty { get; set; }
    public decimal Difference { get; set; }
}
