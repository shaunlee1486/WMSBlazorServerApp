using System;
using WMS.SharedKernel;

namespace WMS.Domain.Entities.MasterData;

public class Product : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    public Guid UnitId { get; set; }
    public Unit Unit { get; set; } = null!;
    
    public Guid? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public decimal MinStock { get; set; } = 0;
    public decimal MaxStock { get; set; } = 0;
    public decimal ReorderPoint { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
