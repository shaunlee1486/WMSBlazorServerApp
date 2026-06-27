using System;
using System.Collections.Generic;
using WMS.Domain.Entities.MasterData;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Domain.Entities.Outbound;

public class SalesOrder : BaseEntity
{
    public string SONumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? RequiredDate { get; set; }
    public string? Note { get; set; }
    public Guid CreatedBy { get; set; }

    // Navigation property
    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
}
