using System.ComponentModel.DataAnnotations;

namespace CentralKitchen.Application.DTOs.Orders;

public class CreateOrderItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    [MaxLength(200)]
    public string? Note { get; set; }
}
