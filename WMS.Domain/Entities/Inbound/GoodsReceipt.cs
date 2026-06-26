using System;
using System.Collections.Generic;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Domain.Entities.Inbound;

public class GoodsReceipt : BaseEntity
{
    public string GRNumber { get; set; } = string.Empty;
    public Guid PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    public Guid ReceivedBy { get; set; }
    public GoodsReceiptStatus Status { get; set; } = GoodsReceiptStatus.Draft;
    public string? Note { get; set; }

    // Navigation property
    public ICollection<GoodsReceiptItem> Items { get; set; } = new List<GoodsReceiptItem>();
}
