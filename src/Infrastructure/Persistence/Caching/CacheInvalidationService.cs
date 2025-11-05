using Persistence.Caching.Contracts;

namespace Persistence.Caching
{
    /// <summary>
    /// Implementation of cache invalidation service that simplifies cache management.
    /// Automatically handles common cache invalidation patterns.
    /// </summary>
    public class CacheInvalidationService : ICacheInvalidationService
    {
        private readonly ICacheKeyService _cacheKeyService;
        private readonly ICacheService _cacheService;

        public CacheInvalidationService(ICacheKeyService cacheKeyService, ICacheService cacheService)
        {
            _cacheKeyService = cacheKeyService;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Invalidates the list cache for the specified entity type.
        /// Uses the entity type name to generate the cache key.
        /// </summary>
        public async Task InvalidateEntityListCacheAsync<TEntity>(CancellationToken cancellationToken = default)
        {
            var entityName = typeof(TEntity).Name;
            var cacheKey = _cacheKeyService.GetListKey(entityName);
            await _cacheService.RemoveAsync(cacheKey, cancellationToken);
        }

        /// <summary>
        /// Invalidates both list cache and related entity cache.
        /// For example, for products with categories, it invalidates:
        /// - Generic product list cache
        /// - Category-specific product cache (if relatedId is provided)
        /// </summary>
        public async Task InvalidateEntityCacheAsync<TEntity>(Guid? relatedId = null, CancellationToken cancellationToken = default)
        {
            // Always invalidate the generic list cache
            await InvalidateEntityListCacheAsync<TEntity>(cancellationToken);

            // If a related ID is provided, also invalidate the related entity cache
            if (relatedId.HasValue)
            {
                var entityName = typeof(TEntity).Name;
                var relatedCacheKey = _cacheKeyService.GetKey($"{entityName}:Category", relatedId.Value);
                await _cacheService.RemoveAsync(relatedCacheKey, cancellationToken);
            }
        }

        /// <summary>
        /// Invalidates a specific cache key.
        /// </summary>
        public async Task InvalidateCacheAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            await _cacheService.RemoveAsync(cacheKey, cancellationToken);
        }

        /// <summary>
        /// Invalidates multiple cache keys in parallel.
        /// </summary>
        public async Task InvalidateCachesAsync(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default)
        {
            var tasks = cacheKeys.Select(key => _cacheService.RemoveAsync(key, cancellationToken));
            await Task.WhenAll(tasks);
        }
    }
}

