using System.ComponentModel.DataAnnotations;

namespace CentralKitchen.Application.DTOs.Stores;

public class UpdateStoreDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(250)]
    public string Address { get; set; } = null!;

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Required]
    public bool IsActive { get; set; }
}
