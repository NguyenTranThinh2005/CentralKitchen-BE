using System;

namespace CentralKitchen.Domain.Entities;

public class OrderStatusLog
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string FromStatus { get; set; } = null!;
    public string ToStatus { get; set; } = null!;
    public Guid ChangedBy { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public Order Order { get; set; } = null!;
    public User Changer { get; set; } = null!;
}
