using System;
using System.ComponentModel.DataAnnotations;
using CentralKitchen.Domain.Enums;

namespace CentralKitchen.Application.DTOs.Products;

public class CreateProductDto
{
    public Guid? Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = null!;

    [MaxLength(500)]
    public string Description { get; set; } = "";

    [Required]
    [MaxLength(50)]
    public string Unit { get; set; } = null!;

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be positive.")]
    public decimal UnitPrice { get; set; }

    [Required]
    public ProductCategory Category { get; set; }

    public bool IsActive { get; set; } = true;
}
