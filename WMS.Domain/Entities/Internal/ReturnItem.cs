using System;
using WMS.Domain.Entities.MasterData;
using WMS.SharedKernel.Enums;

namespace WMS.Domain.Entities.Internal;

public class ReturnItem
{
    public Guid Id { get; set; }
    
    public Guid ReturnId { get; set; }
    public Return Return { get; set; } = null!;
    
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;
    
    public decimal Quantity { get; set; }
    
    public InspectionStatus InspectionStatus { get; set; } = InspectionStatus.Pending;
    
    public string? Note { get; set; }
}
