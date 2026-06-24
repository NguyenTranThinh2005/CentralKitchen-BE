using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CentralKitchen.Application.DTOs.Orders;
using CentralKitchen.Application.Interfaces;
using CentralKitchen.Domain.Entities;
using CentralKitchen.Domain.Enums;

namespace CentralKitchen.Application.Services;

public class OrderService : IOrderService
{
    private readonly IApplicationDbContext _context;

    public OrderService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> CreateOrderAsync(Guid userId, Guid storeId, CreateOrderDto dto)
    {
        var orderCode = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        var order = new Order
        {
            OrderCode = orderCode,
            StoreId = storeId,
            CreatedBy = userId,
            Status = OrderStatus.Created,
            Note = dto.Note,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var itemDto in dto.OrderItems)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == itemDto.ProductId);
            if (product == null)
            {
                throw new ArgumentException($"Product ID {itemDto.ProductId} not found.");
            }

            order.OrderItems.Add(new OrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                Unit = product.Unit,
                UnitPrice = product.UnitPrice,
                Note = itemDto.Note
            });
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Log initial status creation
        _context.OrderStatusLogs.Add(new OrderStatusLog
        {
            OrderId = order.Id,
            FromStatus = "",
            ToStatus = OrderStatus.Created.ToString().ToLower(),
            ChangedBy = userId,
            Note = "Order created",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(order.Id, "manager", null) ?? throw new InvalidOperationException("Failed to retrieve created order.");
    }

    public async Task<List<OrderDto>> GetOrdersAsync(string userRole, Guid? userStoreId, OrderStatus? status = null, DateTime? date = null)
    {
        var query = _context.Orders
            .Include(o => o.Store)
            .Include(o => o.Creator)
            .Include(o => o.Canceller)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.StatusLogs)
            .ThenInclude(l => l.Changer)
            .AsQueryable();

        // Store staff can only see their store's orders
        if (userRole == "store_staff")
        {
            if (userStoreId == null)
            {
                return new List<OrderDto>(); // No store assigned
            }
            query = query.Where(o => o.StoreId == userStoreId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (date.HasValue)
        {
            var startDate = date.Value.Date;
            var endDate = startDate.AddDays(1);
            query = query.Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate);
        }

        var orders = await query.ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid id, string userRole, Guid? userStoreId)
    {
        var query = _context.Orders
            .Include(o => o.Store)
            .Include(o => o.Creator)
            .Include(o => o.Canceller)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.StatusLogs)
            .ThenInclude(l => l.Changer)
            .AsQueryable();

        var order = await query.FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return null;

        // Store staff isolation check
        if (userRole == "store_staff" && order.StoreId != userStoreId)
        {
            return null;
        }

        return MapToDto(order);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateOrderStatusAsync(Guid id, Guid userId, string userRole, Guid? userStoreId, UpdateOrderStatusDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return (false, "Order not found.");
        }

        if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var targetStatus))
        {
            return (false, $"Invalid status: {dto.Status}");
        }

        var currentStatus = order.Status;

        if (currentStatus == targetStatus)
        {
            return (true, null); // No change needed
        }

        // Cancel flow can be triggered from created, accepted, processing
        if (targetStatus == OrderStatus.Cancelled)
        {
            if (currentStatus != OrderStatus.Created && currentStatus != OrderStatus.Accepted && currentStatus != OrderStatus.Processing)
            {
                return (false, $"Cannot cancel order from current state: {currentStatus}");
            }

            if (userRole != "manager" && userRole != "kitchen_staff")
            {
                return (false, "Only managers or kitchen staff can cancel orders.");
            }

            if (string.IsNullOrWhiteSpace(dto.CancelReason))
            {
                return (false, "Cancellation reason is required.");
            }

            order.Status = OrderStatus.Cancelled;
            order.CancelReason = dto.CancelReason;
            order.CancelledBy = userId;
            order.CancelledAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            _context.OrderStatusLogs.Add(new OrderStatusLog
            {
                OrderId = order.Id,
                FromStatus = currentStatus.ToString().ToLower(),
                ToStatus = OrderStatus.Cancelled.ToString().ToLower(),
                ChangedBy = userId,
                Note = dto.Note ?? "Order cancelled",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return (true, null);
        }

        // Validate regular state transitions
        if (targetStatus == OrderStatus.Accepted)
        {
            if (currentStatus != OrderStatus.Created)
                return (false, "Order must be in Created status to accept.");
            if (userRole != "kitchen_staff")
                return (false, "Only kitchen staff can accept orders.");

            order.Status = OrderStatus.Accepted;
            order.AcceptedAt = DateTime.UtcNow;
        }
        else if (targetStatus == OrderStatus.Processing)
        {
            if (currentStatus != OrderStatus.Accepted)
                return (false, "Order must be in Accepted status to start processing.");
            if (userRole != "kitchen_staff")
                return (false, "Only kitchen staff can process orders.");

            order.Status = OrderStatus.Processing;
            order.ProcessingAt = DateTime.UtcNow;
        }
        else if (targetStatus == OrderStatus.Shipping)
        {
            if (currentStatus != OrderStatus.Processing)
                return (false, "Order must be in Processing status to ship.");
            if (userRole != "kitchen_staff")
                return (false, "Only kitchen staff can ship orders.");

            // Deduct stock in inventory
            foreach (var item in order.OrderItems)
            {
                var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.ProductId);
                if (inventory == null)
                {
                    return (false, $"Inventory record for product ID {item.ProductId} not found.");
                }

                if (inventory.Quantity < item.Quantity)
                {
                    return (false, $"Insufficient stock for product ID {item.ProductId}. Available: {inventory.Quantity}, Required: {item.Quantity}");
                }

                inventory.Quantity -= item.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;

                // Log inventory change
                _context.InventoryLogs.Add(new InventoryLog
                {
                    ProductId = item.ProductId,
                    ChangeQty = -item.Quantity,
                    Reason = "order_shipment",
                    ReferenceId = order.OrderCode,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            order.Status = OrderStatus.Shipping;
            order.ShippedAt = DateTime.UtcNow;
        }
        else if (targetStatus == OrderStatus.Received)
        {
            if (currentStatus != OrderStatus.Shipping)
                return (false, "Order must be in Shipping status to receive.");
            if (userRole != "store_staff")
                return (false, "Only store staff can receive orders.");
            if (order.StoreId != userStoreId)
                return (false, "Store staff can only receive orders for their own store.");

            order.Status = OrderStatus.Received;
            order.ReceivedAt = DateTime.UtcNow;
        }
        else
        {
            return (false, "Invalid state transition flow.");
        }

        order.UpdatedAt = DateTime.UtcNow;

        _context.OrderStatusLogs.Add(new OrderStatusLog
        {
            OrderId = order.Id,
            FromStatus = currentStatus.ToString().ToLower(),
            ToStatus = targetStatus.ToString().ToLower(),
            ChangedBy = userId,
            Note = dto.Note ?? $"Order status changed to {targetStatus}",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, null);
    }

    private OrderDto MapToDto(Order o)
    {
        return new OrderDto
        {
            Id = o.Id,
            OrderCode = o.OrderCode,
            StoreId = o.StoreId,
            StoreName = o.Store?.Name ?? "",
            CreatedBy = o.CreatedBy,
            CreatorName = o.Creator?.FullName ?? "",
            Status = o.Status.ToString().ToLower(),
            Note = o.Note,
            CancelReason = o.CancelReason,
            CancelledBy = o.CancelledBy,
            CancellerName = o.Canceller?.FullName,
            AcceptedAt = o.AcceptedAt,
            ProcessingAt = o.ProcessingAt,
            ShippedAt = o.ShippedAt,
            ReceivedAt = o.ReceivedAt,
            CancelledAt = o.CancelledAt,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt,
            OrderItems = o.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? "",
                Quantity = oi.Quantity,
                Unit = oi.Unit,
                UnitPrice = oi.UnitPrice,
                Note = oi.Note
            }).ToList(),
            StatusLogs = o.StatusLogs.Select(sl => new OrderStatusLogDto
            {
                Id = sl.Id,
                FromStatus = sl.FromStatus,
                ToStatus = sl.ToStatus,
                ChangedBy = sl.ChangedBy,
                ChangerName = sl.Changer?.FullName ?? "",
                Note = sl.Note,
                CreatedAt = sl.CreatedAt
            }).OrderBy(l => l.CreatedAt).ToList()
        };
    }
}
