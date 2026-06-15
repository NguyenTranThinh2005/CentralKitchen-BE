using System;

namespace CentralKitchen.Application.DTOs.Orders;

public class OrderStatusLogDto
{
    public int Id { get; set; }
    public string FromStatus { get; set; } = null!;
    public string ToStatus { get; set; } = null!;
    public Guid ChangedBy { get; set; }
    public string ChangerName { get; set; } = null!;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
