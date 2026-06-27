using System;
using System.Collections.Generic;

namespace WMS.Application.Features.SalesOrders.DTOs;

public class SalesOrderDto
{
    public Guid Id { get; set; }
    public string SONumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string? Note { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SalesOrderItemDto> Items { get; set; } = new();
}

public class SalesOrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal OrderedQty { get; set; }
    public decimal PickedQty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => OrderedQty * UnitPrice;
}
