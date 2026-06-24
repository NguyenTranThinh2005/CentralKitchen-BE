using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CentralKitchen.Application.DTOs.Common;
using CentralKitchen.Application.DTOs.Products;
using CentralKitchen.Application.Interfaces;
using CentralKitchen.Domain.Enums;

namespace CentralKitchen.API.Controllers;

/// <summary>
/// Product management endpoints.
/// </summary>
public class ProductsController : ApiControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Gets lists of products with optional category and status filtering.
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), 200)]
    public async Task<IActionResult> GetProducts([FromQuery] ProductCategory? category, [FromQuery] bool? isActive)
    {
        var products = await _productService.GetProductsAsync(category, isActive);
        return Ok(ApiResponse<List<ProductDto>>.Ok(products));
    }

    /// <summary>
    /// Gets a specific product details.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound(ApiResponse<object>.Fail("Product not found."));
        }
        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    /// <summary>
    /// Creates a new product. Automatically initializes the stock quantity to 0 in inventory.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var product = await _productService.CreateProductAsync(dto);
        return Ok(ApiResponse<ProductDto>.Ok(product, "Product created successfully."));
    }

    /// <summary>
    /// Updates product information.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        var product = await _productService.UpdateProductAsync(id, dto);
        if (product == null)
        {
            return NotFound(ApiResponse<object>.Fail("Product not found."));
        }
        return Ok(ApiResponse<ProductDto>.Ok(product, "Product updated successfully."));
    }
}
