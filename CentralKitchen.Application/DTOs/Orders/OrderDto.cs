using System;
using System.Collections.Generic;

namespace CentralKitchen.Application.DTOs.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public int StoreId { get; set; }
    public string StoreName { get; set; } = null!;
    public Guid CreatedBy { get; set; }
    public string CreatorName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Note { get; set; }
    public string? CancelReason { get; set; }
    public Guid? CancelledBy { get; set; }
    public string? CancellerName { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? ProcessingAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<OrderItemDto> OrderItems { get; set; } = new();
    public List<OrderStatusLogDto> StatusLogs { get; set; } = new();
}
