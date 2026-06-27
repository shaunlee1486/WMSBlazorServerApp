using System;

namespace WMS.Application.Features.Dashboard.DTOs;

public class DashboardStatsDto
{
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int PendingPOsCount { get; set; }
    public int OpenSOsCount { get; set; }
}

public class StockValueByCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public decimal TotalQuantity { get; set; }
}

public class InboundOutboundVolumeDto
{
    public DateTime Date { get; set; }
    public decimal InboundQuantity { get; set; }
    public decimal OutboundQuantity { get; set; }
}

public class TopProductDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public int MovementCount { get; set; }
}
