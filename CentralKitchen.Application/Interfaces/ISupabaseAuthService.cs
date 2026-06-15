using System.Threading.Tasks;

namespace CentralKitchen.Application.Interfaces;

public interface ISupabaseAuthService
{
    Task<(string? AccessToken, System.Guid? UserId, string? ErrorMessage)> LoginAsync(string email, string password);
}
