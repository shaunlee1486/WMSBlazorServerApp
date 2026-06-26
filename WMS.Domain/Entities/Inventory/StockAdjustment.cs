using System;
using System.Collections.Generic;
using WMS.Domain.Entities.MasterData;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Domain.Entities.Inventory;

public class StockAdjustment : BaseEntity
{
    public string AdjNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
    public Guid? ApprovedBy { get; set; }
    public AdjustmentStatus Status { get; set; } = AdjustmentStatus.Draft;
    public Guid CreatedBy { get; set; }

    // Navigation property
    public ICollection<StockAdjustmentItem> Items { get; set; } = new List<StockAdjustmentItem>();
}
