using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CentralKitchen.Application.DTOs.Inventory;
using CentralKitchen.Application.Interfaces;
using CentralKitchen.Domain.Entities;

namespace CentralKitchen.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IApplicationDbContext _context;

    public InventoryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InventoryDto>> GetInventoryAsync()
    {
        return await _context.Inventories
            .Include(i => i.Product)
            .Select(i => new InventoryDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                ProductUnit = i.Product.Unit,
                ProductDescription = i.Product.Description,
                Quantity = i.Quantity,
                MinQuantity = i.MinQuantity,
                UpdatedAt = i.UpdatedAt
            }).ToListAsync();
    }

    public async Task<InventoryDto?> UpdateInventoryAsync(Guid productId, Guid userId, UpdateInventoryDto dto)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (inventory == null) return null;

        var oldQty = inventory.Quantity;
        inventory.Quantity = dto.Quantity;
        inventory.MinQuantity = dto.MinQuantity;
        inventory.UpdatedAt = DateTime.UtcNow;

        var diff = dto.Quantity - oldQty;

        // Log manual adjustment
        _context.InventoryLogs.Add(new InventoryLog
        {
            ProductId = productId,
            ChangeQty = diff,
            Reason = "manual_adjust",
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return new InventoryDto
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            ProductName = inventory.Product.Name,
            ProductUnit = inventory.Product.Unit,
            ProductDescription = inventory.Product.Description,
            Quantity = inventory.Quantity,
            MinQuantity = inventory.MinQuantity,
            UpdatedAt = inventory.UpdatedAt
        };
    }
}
