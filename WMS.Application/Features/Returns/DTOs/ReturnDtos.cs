using System;
using System.Collections.Generic;

namespace WMS.Application.Features.Returns.DTOs;

public class ReturnDto
{
    public Guid Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty; // Customer or Supplier
    public string ReferenceNo { get; set; } = string.Empty; // original GR or GI reference
    public string Status { get; set; } = string.Empty; // Pending, Processing, Completed, Cancelled
    public string? Note { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ReturnItemDto> Items { get; set; } = new();
}

public class ReturnItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string LocationBarcode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string InspectionStatus { get; set; } = string.Empty; // Pending, Accepted, Rejected
    public string? Note { get; set; }
}
