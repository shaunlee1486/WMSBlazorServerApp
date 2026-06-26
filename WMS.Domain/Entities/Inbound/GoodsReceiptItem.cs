using System;
using WMS.Domain.Entities.MasterData;

namespace WMS.Domain.Entities.Inbound;

public class GoodsReceiptItem
{
    public Guid Id { get; set; }
    public Guid GoodsReceiptId { get; set; }
    public GoodsReceipt GoodsReceipt { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;
    public decimal ReceivedQty { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
