using System;

namespace CentralKitchen.Application.DTOs.Inventory;

public class InventoryDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string ProductUnit { get; set; } = null!;
    public string? ProductDescription { get; set; }
    public int Quantity { get; set; }
    public int MinQuantity { get; set; }
    public DateTime UpdatedAt { get; set; }
}
