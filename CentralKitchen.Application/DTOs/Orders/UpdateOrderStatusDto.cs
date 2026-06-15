using System.ComponentModel.DataAnnotations;

namespace CentralKitchen.Application.DTOs.Orders;

public class UpdateOrderStatusDto
{
    [Required]
    public string Status { get; set; } = null!;

    [MaxLength(500)]
    public string? CancelReason { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}
