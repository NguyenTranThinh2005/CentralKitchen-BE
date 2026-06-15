using System;
using System.Collections.Generic;

namespace CentralKitchen.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string Role { get; set; } = null!;
    public int? StoreId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public Store? Store { get; set; }
    public ICollection<Order> CreatedOrders { get; set; } = new List<Order>();
    public ICollection<Order> CancelledOrders { get; set; } = new List<Order>();
    public ICollection<OrderStatusLog> StatusChanges { get; set; } = new List<OrderStatusLog>();
    public ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
}
