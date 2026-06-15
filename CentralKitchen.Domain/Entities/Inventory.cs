using System;

namespace CentralKitchen.Domain.Entities;

public class Inventory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int MinQuantity { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public Product Product { get; set; } = null!;
}
