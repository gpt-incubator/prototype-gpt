using PrototypeGPT.Application.Providers;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace PrototypeGPT.Infrastructure;

public class CacheProvider : ICacheProvider
{
    private readonly IDistributedCache cache;

    public CacheProvider(IDistributedCache cache)
    {
        this.cache = cache;
    }

    public async Task<T?> GetFromCacheAsync<T>(string key, CancellationToken cancellation = default) where T : class
    {
        var cachedResponse = await this.cache.GetStringAsync(key, cancellation);

        return cachedResponse == null ? default : JsonSerializer.Deserialize<T>(cachedResponse);
    }

    public async Task SetCacheAsync<T>(string key, T value, TimeSpan? absoluteExpireTime = null, CancellationToken cancellation = default) where T : class
    {
        var options = new DistributedCacheEntryOptions();

        options.AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromMinutes(60);

        var response = JsonSerializer.Serialize(value);
        await this.cache.SetStringAsync(key, response, options, cancellation);
    }

    public async Task ClearCacheAsync(string key, CancellationToken cancellation = default)
    {
        await this.cache.RemoveAsync(key, cancellation);
    }
}
