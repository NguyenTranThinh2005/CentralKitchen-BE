namespace CentralKitchen.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public string? Note { get; set; }

    // Navigation Properties
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
