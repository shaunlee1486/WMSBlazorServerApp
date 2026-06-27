using System;
using System.Collections.Generic;

namespace WMS.Application.Features.PickList.DTOs;

public class PickListDto
{
    public Guid Id { get; set; }
    public string PickListNumber { get; set; } = string.Empty;
    public Guid SalesOrderId { get; set; }
    public string SONumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public Guid? AssignedTo { get; set; }
    public string? AssignedToUserName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<PickListItemDto> Items { get; set; } = new();
}

public class PickListItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string LocationBarcode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty; // e.g. Aisle-Bay-Level-Position
    public decimal RequiredQty { get; set; }
    public decimal PickedQty { get; set; }
    public string Status { get; set; } = string.Empty;
}
