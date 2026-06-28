using System.Collections.Concurrent;

namespace CentralKitchen.API.Security;

public record CachedAuthProfile(string FullName, string Role, Guid? StoreId);

public static class AuthSessionCache
{
    private static readonly ConcurrentDictionary<Guid, CachedAuthProfile> Profiles = new();

    public static void Set(Guid userId, string fullName, string role, Guid? storeId)
    {
        Profiles[userId] = new CachedAuthProfile(fullName, role.ToLower(), storeId);
    }

    public static bool TryGet(Guid userId, out CachedAuthProfile profile)
    {
        return Profiles.TryGetValue(userId, out profile!);
    }
}
