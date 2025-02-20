using Microsoft.Extensions.Caching.Memory;
using System;

public class SessionService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(30); // Matches session timeout in Program.cs

    public SessionService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool IsUserLoggedInElsewhere(string userId, string currentSessionId)
    {
        if (_cache.TryGetValue(userId, out string existingSessionId))
        {
            return existingSessionId != currentSessionId;
        }
        return false;
    }

    public void SetUserSession(string userId, string sessionId)
    {
        _cache.Set(userId, sessionId, _sessionTimeout);
    }

    public void RemoveSession(string userId)
    {
        _cache.Remove(userId);
    }
}
