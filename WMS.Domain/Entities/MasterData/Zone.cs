using System;
using System.Collections.Generic;
using WMS.SharedKernel;
using WMS.SharedKernel.Enums;

namespace WMS.Domain.Entities.MasterData;

public class Zone : BaseEntity
{
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ZoneType ZoneType { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<Location> Locations { get; set; } = new List<Location>();
}
