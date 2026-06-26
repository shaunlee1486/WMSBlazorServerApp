using System.Collections.Generic;
using WMS.SharedKernel;

namespace WMS.Domain.Entities.MasterData;

public class Warehouse : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<Zone> Zones { get; set; } = new List<Zone>();
}
