using System;
using System.Collections.Generic;

namespace WMS.Application.Features.TransferOrders.DTOs;

public class TransferOrderDto
{
    public Guid Id { get; set; }
    public string TONumber { get; set; } = string.Empty;
    public Guid FromWarehouseId { get; set; }
    public string FromWarehouseName { get; set; } = string.Empty;
    public Guid ToWarehouseId { get; set; }
    public string ToWarehouseName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TransferOrderItemDto> Items { get; set; } = new();
}

public class TransferOrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public Guid FromLocationId { get; set; }
    public string FromLocationBarcode { get; set; } = string.Empty;
    public Guid ToLocationId { get; set; }
    public string ToLocationBarcode { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public string Status { get; set; } = string.Empty;
}
