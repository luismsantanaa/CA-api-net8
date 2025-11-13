namespace Persistence.Caching.Contracts
{
    public interface ICacheService
    {
        T? Get<T>(string key);
        Task<T?> GetAsync<T>(string key, CancellationToken token = default);
        void Refresh(string key);
        Task RefreshAsync(string key, CancellationToken token = default);
        void Remove(string key);
        Task RemoveAsync(string key, CancellationToken token = default);
        void Set<T>(string key, T value, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null);
        Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default);
    }
}
