using System;
using System.Collections.Generic;
using WMS.SharedKernel;

namespace WMS.Domain.Entities.MasterData;

public class Category : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    // Navigation properties
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
