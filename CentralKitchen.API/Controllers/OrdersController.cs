using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CentralKitchen.Application.DTOs.Common;
using CentralKitchen.Application.DTOs.Orders;
using CentralKitchen.Application.Interfaces;
using CentralKitchen.Domain.Enums;

namespace CentralKitchen.API.Controllers;

/// <summary>
/// Orders placement and fulfillment status transitions.
/// </summary>
public class OrdersController : ApiControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Submits a new franchise order. Restricted to Store Staff. Status automatically set to "created".
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireStoreStaff")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var storeId = CurrentUserStoreId;
        if (storeId == null)
        {
            return BadRequest(ApiResponse<object>.Fail("Store Staff must be assigned to a specific store location to submit orders."));
        }

        var order = await _orderService.CreateOrderAsync(CurrentUserId, storeId.Value, dto);
        return Ok(ApiResponse<OrderDto>.Ok(order, "Order created successfully."));
    }

    /// <summary>
    /// Gets orders list filtered by status and creation date. Store Staff see only their own store orders, Kitchen Staff and Managers see all.
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), 200)]
    public async Task<IActionResult> GetOrders([FromQuery] OrderStatus? status, [FromQuery] DateTime? date)
    {
        var orders = await _orderService.GetOrdersAsync(CurrentUserRole, CurrentUserStoreId, status, date);
        return Ok(ApiResponse<List<OrderDto>>.Ok(orders));
    }

    /// <summary>
    /// Gets detailed information for a single order, including order items and status history logs.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id, CurrentUserRole, CurrentUserStoreId);
        if (order == null)
        {
            return NotFound(ApiResponse<object>.Fail("Order not found or access is denied."));
        }
        return Ok(ApiResponse<OrderDto>.Ok(order));
    }

    /// <summary>
    /// Patches order fulfillment status state transition. Enforces strict transitions rules:
    /// - created -> accepted (Kitchen Staff only)
    /// - accepted -> processing (Kitchen Staff only)
    /// - processing -> shipping (Kitchen Staff only) -> Triggers stock reduction in inventory and registers log.
    /// - shipping -> received (Store Staff of that store only)
    /// - created/accepted/processing -> cancelled (Manager or Kitchen Staff only, cancelReason required)
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
    {
        var (success, errorMsg) = await _orderService.UpdateOrderStatusAsync(
            id,
            CurrentUserId,
            CurrentUserRole,
            CurrentUserStoreId,
            dto
        );

        if (!success)
        {
            return BadRequest(ApiResponse<object>.Fail(errorMsg ?? "Fulfillment transition validation failed."));
        }

        return Ok(ApiResponse<object>.Ok(null, "Order status updated successfully."));
    }
}
