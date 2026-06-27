using System;

namespace WMS.Domain.Entities.Reporting;

public class StockCategoryStats
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public decimal TotalValue { get; set; }
}

public class ProductMovementStats
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public int MovementCount { get; set; }
}
