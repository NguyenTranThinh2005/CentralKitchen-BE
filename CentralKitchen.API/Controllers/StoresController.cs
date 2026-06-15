using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CentralKitchen.Application.DTOs.Common;
using CentralKitchen.Application.DTOs.Stores;
using CentralKitchen.Application.Interfaces;

namespace CentralKitchen.API.Controllers;

/// <summary>
/// Store location management endpoints.
/// </summary>
public class StoresController : ApiControllerBase
{
    private readonly IStoreService _storeService;

    public StoresController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    /// <summary>
    /// Gets all stores in the franchise.
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<StoreDto>>), 200)]
    public async Task<IActionResult> GetStores()
    {
        var stores = await _storeService.GetStoresAsync();
        return Ok(ApiResponse<List<StoreDto>>.Ok(stores));
    }

    /// <summary>
    /// Gets a store details by identifier.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<StoreDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetStoreById(int id)
    {
        var store = await _storeService.GetStoreByIdAsync(id);
        if (store == null)
        {
            return NotFound(ApiResponse<object>.Fail("Store not found."));
        }
        return Ok(ApiResponse<StoreDto>.Ok(store));
    }

    /// <summary>
    /// Registers a new franchise store.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<StoreDto>), 200)]
    public async Task<IActionResult> CreateStore([FromBody] CreateStoreDto dto)
    {
        var store = await _storeService.CreateStoreAsync(dto);
        return Ok(ApiResponse<StoreDto>.Ok(store, "Store created successfully."));
    }

    /// <summary>
    /// Updates franchise store profile properties.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<StoreDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateStore(int id, [FromBody] UpdateStoreDto dto)
    {
        var store = await _storeService.UpdateStoreAsync(id, dto);
        if (store == null)
        {
            return NotFound(ApiResponse<object>.Fail("Store not found."));
        }
        return Ok(ApiResponse<StoreDto>.Ok(store, "Store updated successfully."));
    }
}
