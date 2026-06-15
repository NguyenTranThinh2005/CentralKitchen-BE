using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        var (token, userId, error) = await _authService.LoginAsync(request.Email, request.Password);

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

        var response = new LoginResponse
        {
            AccessToken = token,
            UserId = userProfile.Id,
            FullName = userProfile.FullName,
            Role = userProfile.Role.ToLower(),
            StoreId = userProfile.StoreId
        };

        return Ok(ApiResponse<LoginResponse>.Ok(response, "Login successful."));
    }
}
