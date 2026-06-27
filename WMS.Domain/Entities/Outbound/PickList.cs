using System;
using System.Collections.Generic;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Domain.Entities.Outbound;

public class PickList : BaseEntity
{
    public string PickListNumber { get; set; } = string.Empty;
    public Guid SalesOrderId { get; set; }
    public SalesOrder SalesOrder { get; set; } = null!;
    public Guid? AssignedTo { get; set; } // Reference to User.Id (Guid)
    public PickListStatus Status { get; set; } = PickListStatus.Pending;

    // Navigation property
    public ICollection<PickListItem> Items { get; set; } = new List<PickListItem>();
}
