using Microsoft.Extensions.DependencyInjection;
using CentralKitchen.Application.Interfaces;
using CentralKitchen.Application.Services;

namespace CentralKitchen.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
