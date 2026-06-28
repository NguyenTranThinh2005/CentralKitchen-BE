using System.Threading.Tasks;

namespace CentralKitchen.Application.Interfaces;

public interface ISupabaseAuthService
{
    Task<(string? AccessToken, string? RefreshToken, System.Guid? UserId, string? ErrorMessage)> LoginAsync(string email, string password);
    Task<(string? AccessToken, string? RefreshToken, System.Guid? UserId, string? ErrorMessage)> RefreshAsync(string refreshToken);
}
