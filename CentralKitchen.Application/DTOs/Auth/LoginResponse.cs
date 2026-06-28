using System;

namespace CentralKitchen.Application.DTOs.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = "";
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = "";
    public string Role { get; set; } = null!;
    public Guid? StoreId { get; set; }
}
