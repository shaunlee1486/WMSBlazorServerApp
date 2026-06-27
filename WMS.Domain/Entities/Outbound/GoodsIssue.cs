using System;
using System.Collections.Generic;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Domain.Entities.Outbound;

public class GoodsIssue : BaseEntity
{
    public string GINumber { get; set; } = string.Empty;
    public Guid SalesOrderId { get; set; }
    public SalesOrder SalesOrder { get; set; } = null!;
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;
    public Guid IssuedBy { get; set; }
    public GoodsIssueStatus Status { get; set; } = GoodsIssueStatus.Draft;
    public string? Note { get; set; }

    // Navigation property
    public ICollection<GoodsIssueItem> Items { get; set; } = new List<GoodsIssueItem>();
}
