using System;
using System.Collections.Generic;

namespace WMS.Application.Features.GoodsIssue.DTOs;

public class GoodsIssueDto
{
    public Guid Id { get; set; }
    public string GINumber { get; set; } = string.Empty;
    public Guid SalesOrderId { get; set; }
    public string SONumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public Guid IssuedBy { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<GoodsIssueItemDto> Items { get; set; } = new();
}

public class GoodsIssueItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string LocationBarcode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public decimal IssuedQty { get; set; }
    public string? BatchNo { get; set; }
}
