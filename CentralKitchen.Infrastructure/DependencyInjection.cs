using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CentralKitchen.Application.Interfaces;
using CentralKitchen.Infrastructure.Persistence;
using CentralKitchen.Infrastructure.Services;

namespace CentralKitchen.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Register HttpClient for SupabaseAuthService
        services.AddHttpClient<ISupabaseAuthService, SupabaseAuthService>();

        return services;
    }
}
