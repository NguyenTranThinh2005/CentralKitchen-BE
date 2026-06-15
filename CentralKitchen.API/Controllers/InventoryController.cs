using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CentralKitchen.Application.DTOs.Common;
using CentralKitchen.Application.DTOs.Inventory;
using CentralKitchen.Application.Interfaces;

namespace CentralKitchen.API.Controllers;

/// <summary>
/// Kitchen inventory stock tracking.
/// </summary>
public class InventoryController : ApiControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>
    /// Gets all products stock levels. Accessible by Managers and Kitchen Staff.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequireManagerOrKitchenStaff")]
    [ProducesResponseType(typeof(ApiResponse<List<InventoryDto>>), 200)]
    public async Task<IActionResult> GetInventory()
    {
        var list = await _inventoryService.GetInventoryAsync();
        return Ok(ApiResponse<List<InventoryDto>>.Ok(list));
    }

    /// <summary>
    /// Updates product stock levels manually. Restricted to Managers. Registers inventory log.
    /// </summary>
    [HttpPut("{productId}")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<InventoryDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateInventory(int productId, [FromBody] UpdateInventoryDto dto)
    {
        var inventory = await _inventoryService.UpdateInventoryAsync(productId, CurrentUserId, dto);
        if (inventory == null)
        {
            return NotFound(ApiResponse<object>.Fail("Inventory record for target product not found."));
        }
        return Ok(ApiResponse<InventoryDto>.Ok(inventory, "Inventory manually adjusted successfully."));
    }
}
