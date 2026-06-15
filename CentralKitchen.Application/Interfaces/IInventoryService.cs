using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CentralKitchen.Application.DTOs.Inventory;

namespace CentralKitchen.Application.Interfaces;

public interface IInventoryService
{
    Task<List<InventoryDto>> GetInventoryAsync();
    Task<InventoryDto?> UpdateInventoryAsync(int productId, Guid userId, UpdateInventoryDto dto);
}
