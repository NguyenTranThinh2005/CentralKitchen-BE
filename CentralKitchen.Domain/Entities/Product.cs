using System;
using CentralKitchen.Domain.Enums;

namespace CentralKitchen.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Unit { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public ProductCategory Category { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public Inventory? Inventory { get; set; }
}
