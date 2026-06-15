using System.ComponentModel.DataAnnotations;

namespace CentralKitchen.Application.DTOs.Auth;

public class LoginRequest
{
    /// <summary>
    /// User email address.
    /// </summary>
    /// <example>staff@store.com</example>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "staff@store.com";

    /// <summary>
    /// User password.
    /// </summary>
    /// <example>PRM393_Group3</example>
    [Required]
    public string Password { get; set; } = "PRM393_Group3";
}
