using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CentralKitchen.Application.Interfaces;
using CentralKitchen.Domain.Entities;
using CentralKitchen.Domain.Enums;

namespace CentralKitchen.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<InventoryLog> InventoryLogs => Set<InventoryLog>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusLog> OrderStatusLogs => Set<OrderStatusLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Store Entity
        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        // Configure Product Entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.Category)
                .HasConversion(
                    v => v.ToString().ToLower(),
                    v => (ProductCategory)Enum.Parse(typeof(ProductCategory), v, true)
                );
        });

        // Configure Inventory Entity
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Product)
                .WithOne(p => p.Inventory)
                .HasForeignKey<Inventory>(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure InventoryLog Entity
        modelBuilder.Entity<InventoryLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ReferenceId).HasMaxLength(100);
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Creator)
                .WithMany(u => u.InventoryLogs)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure User Entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.Store)
                .WithMany(s => s.Users)
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Order Entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderCode).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.CancelReason).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasConversion(
                    v => v.ToString().ToLower(),
                    v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v, true)
                );

            entity.HasOne(e => e.Store)
                .WithMany(s => s.Orders)
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Creator)
                .WithMany(u => u.CreatedOrders)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Canceller)
                .WithMany(u => u.CancelledOrders)
                .HasForeignKey(e => e.CancelledBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure OrderItem Entity
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.Note).HasMaxLength(200);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure OrderStatusLog Entity
        modelBuilder.Entity<OrderStatusLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FromStatus).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ToStatus).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(500);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.StatusLogs)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Changer)
                .WithMany(u => u.StatusChanges)
                .HasForeignKey(e => e.ChangedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Convert table and column names to snake_case
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Table name mapping
            var tableName = entity.GetTableName();
            if (tableName != null)
            {
                entity.SetTableName(ToSnakeCase(tableName));
            }

            // Column name mapping
            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (columnName != null)
                {
                    property.SetColumnName(ToSnakeCase(columnName));
                }
            }
        }
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var startUnderscore = Regex.Match(input, @"^_+");
        return startUnderscore + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }
}
