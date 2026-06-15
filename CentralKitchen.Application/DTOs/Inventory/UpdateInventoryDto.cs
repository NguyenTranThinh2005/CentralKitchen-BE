using System.ComponentModel.DataAnnotations;

namespace CentralKitchen.Application.DTOs.Inventory;

public class UpdateInventoryDto
{
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
    public int Quantity { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Min quantity cannot be negative.")]
    public int MinQuantity { get; set; }
}
