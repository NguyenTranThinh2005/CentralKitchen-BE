using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CentralKitchen.Application.DTOs.Dashboard;
using CentralKitchen.Application.DTOs.Inventory;
using CentralKitchen.Application.Interfaces;

namespace CentralKitchen.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;

    public DashboardService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var startOfToday = DateTime.UtcNow.Date;
        var endOfToday = startOfToday.AddDays(1);

        var ordersToday = await _context.Orders
            .Where(o => o.CreatedAt >= startOfToday && o.CreatedAt < endOfToday)
            .ToListAsync();

        var totalOrders = ordersToday.Count;

        var ordersByStatus = ordersToday
            .GroupBy(o => o.Status.ToString().ToLower())
            .ToDictionary(g => g.Key, g => g.Count());

        // Low stock products
        var lowStock = await _context.Inventories
            .Include(i => i.Product)
            .Where(i => i.Quantity < i.MinQuantity)
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

        return new DashboardSummaryDto
        {
            TotalOrders = totalOrders,
            OrdersByStatus = ordersByStatus,
            LowStockProducts = lowStock
        };
    }
}
