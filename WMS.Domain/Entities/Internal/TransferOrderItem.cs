using System;
using WMS.Domain.Entities.MasterData;
using WMS.SharedKernel.Enums;

namespace WMS.Domain.Entities.Internal;

public class TransferOrderItem
{
    public Guid Id { get; set; }
    
    public Guid TransferOrderId { get; set; }
    public TransferOrder TransferOrder { get; set; } = null!;
    
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    public Guid FromLocationId { get; set; }
    public Location FromLocation { get; set; } = null!;
    
    public Guid ToLocationId { get; set; }
    public Location ToLocation { get; set; } = null!;
    
    public decimal Qty { get; set; }
    
    public TransferOrderItemStatus Status { get; set; } = TransferOrderItemStatus.Pending;
}
