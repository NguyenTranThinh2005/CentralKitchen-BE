using System.ComponentModel.DataAnnotations;

namespace CentralKitchen.Application.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}
