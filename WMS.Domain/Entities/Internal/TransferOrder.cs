using System;
using System.Collections.Generic;
using WMS.Domain.Entities.MasterData;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Domain.Entities.Internal;

public class TransferOrder : BaseEntity
{
    public string TONumber { get; set; } = string.Empty;
    
    public Guid FromWarehouseId { get; set; }
    public Warehouse FromWarehouse { get; set; } = null!;
    
    public Guid ToWarehouseId { get; set; }
    public Warehouse ToWarehouse { get; set; } = null!;
    
    public TransferOrderStatus Status { get; set; } = TransferOrderStatus.Draft;
    
    public Guid RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }

    // Navigation property
    public ICollection<TransferOrderItem> Items { get; set; } = new List<TransferOrderItem>();
}
