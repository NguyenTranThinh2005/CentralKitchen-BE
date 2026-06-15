using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using CentralKitchen.Application.Interfaces;

namespace CentralKitchen.API.Security;

public class SupabaseClaimsTransformation : IClaimsTransformation
{
    private readonly IServiceProvider _serviceProvider;

    public SupabaseClaimsTransformation(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity == null || !principal.Identity.IsAuthenticated)
        {
            return principal;
        }

        // Avoid repeating claims enrichment if already transformed
        if (principal.HasClaim(c => c.Type == ClaimTypes.Role))
        {
            return principal;
        }

        // Sub claim represents Supabase User UID
        var subClaim = principal.FindFirst("sub") ?? principal.FindFirst(ClaimTypes.NameIdentifier);
        if (subClaim == null || !Guid.TryParse(subClaim.Value, out var supabaseUserId))
        {
            return principal;
        }

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var user = await context.Users.FindAsync(supabaseUserId);
        if (user == null || !user.IsActive)
        {
            return principal; // User not registered or is inactive in local db
        }

        var identity = (ClaimsIdentity)principal.Identity;

        // Add role claim based on db role
        identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToLower()));

        // Add store_id claim if associated with a store
        if (user.StoreId.HasValue)
        {
            identity.AddClaim(new Claim("store_id", user.StoreId.Value.ToString()));
        }

        return principal;
    }
}
