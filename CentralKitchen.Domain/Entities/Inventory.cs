using System;

namespace CentralKitchen.Domain.Entities;

public class Inventory
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public int MinQuantity { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public Product Product { get; set; } = null!;
}
