namespace PrototypeGPT.Application.Providers;

public interface ICacheProvider
{
    Task<T?> GetFromCacheAsync<T>(string key, CancellationToken cancellation = default) where T : class;

    Task SetCacheAsync<T>(string key, T value, TimeSpan? absoluteExpireTime = null, CancellationToken cancellation = default) where T : class;

    Task ClearCacheAsync(string key, CancellationToken cancellation = default);
}
