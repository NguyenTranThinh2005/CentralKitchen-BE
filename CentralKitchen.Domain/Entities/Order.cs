using System;
using System.Collections.Generic;
using CentralKitchen.Domain.Enums;

namespace CentralKitchen.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public Guid StoreId { get; set; }
    public Guid CreatedBy { get; set; }
    public OrderStatus Status { get; set; }
    public string? Note { get; set; }
    public string? CancelReason { get; set; }
    public Guid? CancelledBy { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? ProcessingAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public Store Store { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public User? Canceller { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusLog> StatusLogs { get; set; } = new List<OrderStatusLog>();
}
