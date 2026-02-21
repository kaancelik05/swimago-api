using Microsoft.Extensions.Caching.Memory;
using Swimago.Application.Interfaces;

namespace Swimago.Infrastructure.Services;

public class MemoryCacheTokenBlacklistService : ITokenBlacklistService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheTokenBlacklistService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task BlacklistTokenAsync(string token, TimeSpan expiresIn)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiresIn
        };

        _cache.Set($"blacklist_{token}", true, cacheEntryOptions);
        return Task.CompletedTask;
    }

    public Task<bool> IsTokenBlacklistedAsync(string token)
    {
        var isBlacklisted = _cache.TryGetValue($"blacklist_{token}", out _);
        return Task.FromResult(isBlacklisted);
    }
}
