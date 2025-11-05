namespace Persistence.Caching.Contracts
{
    /// <summary>
    /// Service for automatically invalidating entity caches.
    /// Simplifies cache invalidation by providing common patterns.
    /// </summary>
    public interface ICacheInvalidationService
    {
        /// <summary>
        /// Invalidates the list cache for the specified entity type.
        /// </summary>
        /// <typeparam name="TEntity">The entity type (used to determine cache keys)</typeparam>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task InvalidateEntityListCacheAsync<TEntity>(CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidates both list cache and related entity cache (e.g., category-specific cache).
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <param name="relatedId">Optional related entity ID (e.g., CategoryId for products)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task InvalidateEntityCacheAsync<TEntity>(Guid? relatedId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidates a specific cache key.
        /// </summary>
        /// <param name="cacheKey">The cache key to invalidate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task InvalidateCacheAsync(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidates multiple cache keys.
        /// </summary>
        /// <param name="cacheKeys">The cache keys to invalidate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task InvalidateCachesAsync(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default);
    }
}

