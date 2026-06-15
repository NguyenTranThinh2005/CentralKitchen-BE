using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentralKitchen.Application.DTOs.Orders;

public class CreateOrderDto
{
    [MaxLength(500)]
    public string? Note { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "An order must contain at least one item.")]
    public List<CreateOrderItemDto> OrderItems { get; set; } = null!;
}
