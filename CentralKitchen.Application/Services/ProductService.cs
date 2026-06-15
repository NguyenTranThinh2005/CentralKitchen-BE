using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CentralKitchen.Application.DTOs.Products;
using CentralKitchen.Application.Interfaces;
using CentralKitchen.Domain.Entities;
using CentralKitchen.Domain.Enums;

namespace CentralKitchen.Application.Services;

public class ProductService : IProductService
{
    private readonly IApplicationDbContext _context;

    public ProductService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> GetProductsAsync(ProductCategory? category = null, bool? isActive = null)
    {
        var query = _context.Products.AsQueryable();

        if (category.HasValue)
        {
            query = query.Where(p => p.Category == category.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        return await query.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Unit = p.Unit,
            UnitPrice = p.UnitPrice,
            Category = p.Category.ToString().ToLower(),
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToListAsync();
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var p = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return null;

        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Unit = p.Unit,
            UnitPrice = p.UnitPrice,
            Category = p.Category.ToString().ToLower(),
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description ?? "",
            Unit = dto.Unit,
            UnitPrice = dto.UnitPrice,
            Category = dto.Category,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Automatically initialize stock in inventory
        var inventory = new Inventory
        {
            ProductId = product.Id,
            Quantity = 0,
            MinQuantity = 0,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Unit = product.Unit,
            UnitPrice = product.UnitPrice,
            Category = product.Category.ToString().ToLower(),
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return null;

        product.Name = dto.Name;
        product.Description = dto.Description ?? "";
        product.Unit = dto.Unit;
        product.UnitPrice = dto.UnitPrice;
        product.Category = dto.Category;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Unit = product.Unit,
            UnitPrice = product.UnitPrice,
            Category = product.Category.ToString().ToLower(),
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
