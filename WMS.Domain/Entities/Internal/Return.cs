using System;
using System.Collections.Generic;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Domain.Entities.Internal;

public class Return : BaseEntity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public ReturnType ReturnType { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public ReturnStatus Status { get; set; } = ReturnStatus.Pending;
    public string? Note { get; set; }
    public Guid CreatedBy { get; set; }

    // Navigation property
    public ICollection<ReturnItem> Items { get; set; } = new List<ReturnItem>();
}
