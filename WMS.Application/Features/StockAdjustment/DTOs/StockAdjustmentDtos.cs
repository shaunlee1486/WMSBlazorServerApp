using System;
using System.Collections.Generic;

namespace WMS.Application.Features.StockAdjustment.DTOs;

public class StockAdjustmentDto
{
    public Guid Id { get; set; }
    public string AdjNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public string? Reason { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<StockAdjustmentItemDto> Items { get; set; } = new();
}

public class StockAdjustmentItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string LocationBarcode { get; set; } = string.Empty;
    public decimal SystemQty { get; set; }
    public decimal ActualQty { get; set; }
    public decimal Difference { get; set; }
}
