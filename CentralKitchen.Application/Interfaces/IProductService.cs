using System.Collections.Generic;
using System.Threading.Tasks;
using CentralKitchen.Application.DTOs.Products;
using CentralKitchen.Domain.Enums;

namespace CentralKitchen.Application.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetProductsAsync(ProductCategory? category = null, bool? isActive = null);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto);
}
