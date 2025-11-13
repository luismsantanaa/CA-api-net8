using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Caching.Contracts;

namespace Persistence.Caching
{
    public class LocalCacheService : ICacheService
    {
        private readonly ILogger<LocalCacheService> _logger;
        private readonly IMemoryCache _cache;
        private readonly int? _absoluteExpirationInMinute;
        private readonly int? _slidingExpirationInMinute;

        public LocalCacheService(IMemoryCache cache, IConfiguration configuration, ILogger<LocalCacheService> logger)
        {
            _logger = logger;
            _cache = cache;
            _absoluteExpirationInMinute = configuration.GetValue<int>("CacheSettings:AbsoluteExpirationInMinute");
            _slidingExpirationInMinute = configuration.GetValue<int>("CacheSettings:SlidingExpirationInMinute");
        }

        public T? Get<T>(string key)
        {
            try
            {
                _logger.LogDebug("Getting cache key:{key}", key);
                return _cache.Get<T>(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache key: {Key}", key);
                throw;
            }
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken token = default)
        {
            try
            {
                _logger.LogDebug("Getting async cache key:{key}", key);
                return await Task.FromResult(Get<T>(key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting async cache key: {Key}", key);
                throw;
            }
        }

        public void Refresh(string key)
        {
            try
            {
                _logger.LogDebug("Refreshing cache key:{key}", key);
                _cache.TryGetValue(key, out _);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cache key: {Key}", key);
                throw;
            }
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            try
            {
                _logger.LogDebug("Refreshing async cache key:{key}", key);
                Refresh(key);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing async cache key: {Key}", key);
                throw;
            }
        }

        public void Remove(string key)
        {
            try
            {
                _logger.LogDebug("Remove cache key:{key}", key);
                _cache.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {Key}", key);
                throw;
            }
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            try
            {
                _logger.LogDebug("Remove async cache key:{key}", key);
                Remove(key);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing async cache key: {Key}", key);
                throw;
            }
        }

        public void Set<T>(string key, T value, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null)
        {
            try
            {
                _logger.LogDebug("Setting cache key: {key}", key);
                if (slidingExpiration is null)
                {
                    slidingExpiration = TimeSpan.FromMinutes(10); // Default expiration time of 10 minutes.
                }

                _cache.Set(key, value, GetOptions(slidingExpiration, absoluteExpiration));
                _logger.LogDebug("Added to Cache: {key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache key: {Key}", key);
                throw;
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Setting async cache key:{key}", key);
                Set(key, value, slidingExpiration, absoluteExpiration);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting async cache key: {Key}", key);
                throw;
            }
        }

        private MemoryCacheEntryOptions GetOptions(TimeSpan? slidingExpiration, TimeSpan? absoluteExpiration)
        {
            var options = new MemoryCacheEntryOptions();

            if (absoluteExpiration.HasValue)
            {
                options.SetAbsoluteExpiration(absoluteExpiration.Value);
            }
            else
            {
                if (_absoluteExpirationInMinute.HasValue && _absoluteExpirationInMinute > 0)
                {
                    TimeSpan duration;
                    duration = TimeSpan.FromMinutes((int)_absoluteExpirationInMinute);
                    options.SetAbsoluteExpiration(duration);
                }
                else
                {
                    options.SetAbsoluteExpiration(TimeSpan.FromMinutes(35)); // Default expiration time.
                }
            }

            if (slidingExpiration.HasValue)
            {
                options.SetSlidingExpiration(slidingExpiration.Value);
            }
            else
            {
                if (_slidingExpirationInMinute.HasValue && _slidingExpirationInMinute > 0)
                {
                    TimeSpan duration;
                    duration = TimeSpan.FromMinutes((int)_slidingExpirationInMinute);
                    options.SetSlidingExpiration(duration);
                }
                else
                {
                    options.SetSlidingExpiration(TimeSpan.FromMinutes(30)); // Default expiration time.
                }
            }

            return options;
        }
    }
}
