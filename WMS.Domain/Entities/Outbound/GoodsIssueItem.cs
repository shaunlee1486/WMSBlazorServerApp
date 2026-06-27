using System;
using WMS.Domain.Entities.MasterData;

namespace WMS.Domain.Entities.Outbound;

public class GoodsIssueItem
{
    public Guid Id { get; set; }
    public Guid GoodsIssueId { get; set; }
    public GoodsIssue GoodsIssue { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;
    public decimal IssuedQty { get; set; }
    public string? BatchNo { get; set; }
}
