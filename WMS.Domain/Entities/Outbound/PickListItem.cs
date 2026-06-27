using System;
using WMS.Domain.Entities.MasterData;

namespace WMS.Domain.Entities.Outbound;

public class PickListItem
{
    public Guid Id { get; set; }
    public Guid PickListId { get; set; }
    public PickList PickList { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;
    public decimal RequiredQty { get; set; }
    public decimal PickedQty { get; set; }
    public string Status { get; set; } = "Pending"; // e.g. Pending, Completed
}
