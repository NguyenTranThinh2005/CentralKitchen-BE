using System.Collections.Generic;
using CentralKitchen.Application.DTOs.Inventory;

namespace CentralKitchen.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalOrders { get; set; }
    public Dictionary<string, int> OrdersByStatus { get; set; } = new();
    public List<InventoryDto> LowStockProducts { get; set; } = new();
}
