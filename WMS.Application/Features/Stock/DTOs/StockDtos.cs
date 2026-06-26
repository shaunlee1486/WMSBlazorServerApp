using System;

namespace WMS.Application.Features.Stock.DTOs;

public class StockOverviewDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string LocationBarcode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}

public class StockMovementDto
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? FromLocationBarcode { get; set; }
    public string? ToLocationBarcode { get; set; }
    public decimal Quantity { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LowStockDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal MinStock { get; set; }
    public decimal ReorderPoint { get; set; }
}
