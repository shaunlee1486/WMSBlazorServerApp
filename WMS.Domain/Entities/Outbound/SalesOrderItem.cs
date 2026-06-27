using System;
using WMS.Domain.Entities.MasterData;

namespace WMS.Domain.Entities.Outbound;

public class SalesOrderItem
{
    public Guid Id { get; set; }
    public Guid SalesOrderId { get; set; }
    public SalesOrder SalesOrder { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public decimal OrderedQty { get; set; }
    public decimal PickedQty { get; set; }
    public decimal UnitPrice { get; set; }
}
