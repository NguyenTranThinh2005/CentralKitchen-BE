using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CentralKitchen.Application.DTOs.Orders;
using CentralKitchen.Domain.Enums;

namespace CentralKitchen.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(Guid userId, int storeId, CreateOrderDto dto);
    Task<List<OrderDto>> GetOrdersAsync(string userRole, int? userStoreId, OrderStatus? status = null, DateTime? date = null);
    Task<OrderDto?> GetOrderByIdAsync(int id, string userRole, int? userStoreId);
    Task<(bool Success, string? ErrorMessage)> UpdateOrderStatusAsync(int id, Guid userId, string userRole, int? userStoreId, UpdateOrderStatusDto dto);
}
