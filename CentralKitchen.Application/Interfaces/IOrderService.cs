using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CentralKitchen.Application.DTOs.Orders;
using CentralKitchen.Domain.Enums;

namespace CentralKitchen.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(Guid userId, Guid storeId, CreateOrderDto dto);
    Task<List<OrderDto>> GetOrdersAsync(string userRole, Guid? userStoreId, OrderStatus? status = null, DateTime? date = null);
    Task<OrderDto?> GetOrderByIdAsync(Guid id, string userRole, Guid? userStoreId);
    Task<(bool Success, string? ErrorMessage)> UpdateOrderStatusAsync(Guid id, Guid userId, string userRole, Guid? userStoreId, UpdateOrderStatusDto dto);
}
