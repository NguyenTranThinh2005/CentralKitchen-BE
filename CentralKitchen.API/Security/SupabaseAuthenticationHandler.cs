using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CentralKitchen.API.Security;

public class SupabaseAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public SupabaseAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var accessToken = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Task.FromResult(AuthenticateResult.Fail("Bearer token is empty."));
        }

        try
        {
            var parts = accessToken.Split('.');
            if (parts.Length < 2)
            {
                return Task.FromResult(AuthenticateResult.Fail("Bearer token is malformed."));
            }

            using var payload = JsonDocument.Parse(DecodeBase64Url(parts[1]));
            var root = payload.RootElement;

            if (!root.TryGetProperty("sub", out var subProperty))
            {
                return Task.FromResult(AuthenticateResult.Fail("Supabase user id is missing."));
            }

            var userId = subProperty.GetString();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Task.FromResult(AuthenticateResult.Fail("Supabase user id is empty."));
            }

            if (root.TryGetProperty("exp", out var expProperty) && expProperty.TryGetInt64(out var exp))
            {
                var expiresAt = DateTimeOffset.FromUnixTimeSeconds(exp);
                if (expiresAt <= DateTimeOffset.UtcNow)
                {
                    return Task.FromResult(AuthenticateResult.Fail("Supabase access token is expired."));
                }
            }

            var claims = new List<Claim>
            {
                new("sub", userId),
                new(ClaimTypes.NameIdentifier, userId),
            };

            var email = "";
            if (root.TryGetProperty("email", out var emailProperty))
            {
                email = emailProperty.GetString() ?? "";
                if (!string.IsNullOrWhiteSpace(email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, email));
                }
            }

            if (Guid.TryParse(userId, out var parsedUserId) && AuthSessionCache.TryGet(parsedUserId, out var cachedProfile))
            {
                claims.Add(new Claim(ClaimTypes.Role, cachedProfile.Role));
                if (cachedProfile.StoreId.HasValue)
                {
                    claims.Add(new Claim("store_id", cachedProfile.StoreId.Value.ToString()));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, InferRole(email)));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            return Task.FromResult(AuthenticateResult.Fail($"Bearer token could not be read: {ex.Message}"));
        }
    }

    private static string InferRole(string email)
    {
        var normalized = email.ToLowerInvariant();
        if (normalized.Contains("manager") || normalized.Contains("admin")) return "manager";
        if (normalized.Contains("kitchen")) return "kitchen_staff";
        return "store_staff";
    }

    private static string DecodeBase64Url(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2:
                padded += "==";
                break;
            case 3:
                padded += "=";
                break;
        }

        var bytes = Convert.FromBase64String(padded);
        return Encoding.UTF8.GetString(bytes);
    }
}
