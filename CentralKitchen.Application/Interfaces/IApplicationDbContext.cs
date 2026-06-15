using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CentralKitchen.Domain.Entities;

namespace CentralKitchen.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Store> Stores { get; }
    DbSet<Product> Products { get; }
    DbSet<Inventory> Inventories { get; }
    DbSet<InventoryLog> InventoryLogs { get; }
    DbSet<User> Users { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderStatusLog> OrderStatusLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
