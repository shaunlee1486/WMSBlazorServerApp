using System;
using WMS.SharedKernel;

namespace WMS.Domain.Entities.MasterData;

public class Location : BaseEntity
{
    public Guid ZoneId { get; set; }
    public Zone Zone { get; set; } = null!;
    public string? Aisle { get; set; }
    public string? Bay { get; set; }
    public string? Level { get; set; }
    public string? Position { get; set; }
    public string? Barcode { get; set; }
    public decimal? MaxCapacity { get; set; }
    public bool IsActive { get; set; } = true;
}
