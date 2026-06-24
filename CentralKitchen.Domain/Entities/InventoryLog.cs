using System;

namespace CentralKitchen.Domain.Entities;

public class InventoryLog
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int ChangeQty { get; set; }
    public string Reason { get; set; } = null!;
    public string? ReferenceId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public Product Product { get; set; } = null!;
    public User Creator { get; set; } = null!;
}
