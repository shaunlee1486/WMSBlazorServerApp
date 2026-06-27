using System;

namespace WMS.Application.Features.Reports.DTOs;

public class StockSnapshotDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string LocationBarcode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity => Quantity - ReservedQuantity;
}

public class MovementReportDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string FromLocationBarcode { get; set; } = string.Empty;
    public string ToLocationBarcode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; }
    public string? Note { get; set; }
}

public class InboundReportDto
{
    public string GRNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string PONumber { get; set; } = string.Empty;
    public DateTime ReceivedDate { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal ReceivedQty { get; set; }
    public string LocationBarcode { get; set; } = string.Empty;
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class OutboundReportDto
{
    public string GINumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string SONumber { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal IssuedQty { get; set; }
    public string LocationBarcode { get; set; } = string.Empty;
    public string? BatchNo { get; set; }
}

public class AdjustmentReportDto
{
    public string AdjustmentNumber { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedByEmail { get; set; } = string.Empty;
    public string ApprovedByEmail { get; set; } = string.Empty;
    
    // Line items detail
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string LocationBarcode { get; set; } = string.Empty;
    public decimal SystemQty { get; set; }
    public decimal ActualQty { get; set; }
    public decimal DifferenceQty => ActualQty - SystemQty;
}

public class AuditLogReportDto
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
