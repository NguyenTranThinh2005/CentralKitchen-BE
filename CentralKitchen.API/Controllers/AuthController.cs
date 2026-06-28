using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CentralKitchen.API.Security;
using CentralKitchen.Application.DTOs.Auth;
using CentralKitchen.Application.DTOs.Common;
using CentralKitchen.Application.Interfaces;

namespace CentralKitchen.API.Controllers;

/// <summary>
/// Authentication operations.
/// </summary>
public class AuthController : ApiControllerBase
{
    private readonly ISupabaseAuthService _authService;
    private readonly IApplicationDbContext _context;

    public AuthController(ISupabaseAuthService authService, IApplicationDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    /// <summary>
    /// Authenticates a user using email and password against Supabase Auth, returning access token and database profile details.
    /// </summary>
    /// <param name="request">Email and Password payload.</param>
    /// <returns>Login response envelope with JWT AccessToken.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (token, refreshToken, userId, error) = await _authService.LoginAsync(request.Email, request.Password);

        if (!string.IsNullOrEmpty(error) || token == null || userId == null)
        {
            return Unauthorized(ApiResponse<object>.Fail(error ?? "Authentication failed."));
        }

        // Retrieve local profile from PostgreSQL DB
        var userProfile = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (userProfile == null)
        {
            return Unauthorized(ApiResponse<object>.Fail("Authenticated user profile was not found in the local kitchen database."));
        }

        if (!userProfile.IsActive)
        {
            return BadRequest(ApiResponse<object>.Fail("User profile account is currently inactive."));
        }

        AuthSessionCache.Set(userProfile.Id, userProfile.FullName, userProfile.Role, userProfile.StoreId);

        var response = new LoginResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken ?? "",
            UserId = userProfile.Id,
            FullName = userProfile.FullName,
            Email = request.Email,
            Role = userProfile.Role.ToLower(),
            StoreId = userProfile.StoreId
        };

        return Ok(ApiResponse<LoginResponse>.Ok(response, "Login successful."));
    }

    /// <summary>
    /// Refreshes a Supabase access token using a refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var (token, refreshToken, userId, error) = await _authService.RefreshAsync(request.RefreshToken);

        if (!string.IsNullOrEmpty(error) || token == null || userId == null)
        {
            return Unauthorized(ApiResponse<object>.Fail(error ?? "Refresh token failed."));
        }

        var userProfile = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (userProfile == null || !userProfile.IsActive)
        {
            return Unauthorized(ApiResponse<object>.Fail("Authenticated user profile was not found or inactive."));
        }

        var response = new LoginResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken ?? request.RefreshToken,
            UserId = userProfile.Id,
            FullName = userProfile.FullName,
            Email = "",
            Role = userProfile.Role.ToLower(),
            StoreId = userProfile.StoreId
        };

        return Ok(ApiResponse<LoginResponse>.Ok(response, "Token refreshed successfully."));
    }


    /// <summary>
    /// Returns the current authenticated user profile from the local kitchen database.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Me()
    {
        if (CurrentUserId == Guid.Empty)
        {
            return Unauthorized(ApiResponse<object>.Fail("Authenticated user id was not found."));
        }

        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        if (AuthSessionCache.TryGet(CurrentUserId, out var cachedProfile))
        {
            var cachedResponse = new LoginResponse
            {
                AccessToken = "",
                UserId = CurrentUserId,
                FullName = cachedProfile.FullName,
                Email = email,
                Role = cachedProfile.Role,
                StoreId = cachedProfile.StoreId
            };

            return Ok(ApiResponse<LoginResponse>.Ok(cachedResponse));
        }

        var userProfile = await _context.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId);
        if (userProfile == null || !userProfile.IsActive)
        {
            return Unauthorized(ApiResponse<object>.Fail("Authenticated user profile was not found or inactive."));
        }

        AuthSessionCache.Set(userProfile.Id, userProfile.FullName, userProfile.Role, userProfile.StoreId);

        var response = new LoginResponse
        {
            AccessToken = "",
            UserId = userProfile.Id,
            FullName = userProfile.FullName,
            Email = email,
            Role = userProfile.Role.ToLower(),
            StoreId = userProfile.StoreId
        };

        return Ok(ApiResponse<LoginResponse>.Ok(response));
    }

}
