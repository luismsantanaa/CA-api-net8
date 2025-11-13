using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Caching.Contracts;

using Shared.Services.Contracts;

namespace Persistence.Caching
{
    public class DistributedCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<DistributedCacheService> _logger;
        private readonly IJsonService _serializer;
        private readonly int? _absoluteExpirationInMinute;
        private readonly int? _slidingExpirationInMinute;

        public DistributedCacheService(IDistributedCache cache, IJsonService serializer, IConfiguration configuration, ILogger<DistributedCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
            _serializer = serializer;
            _absoluteExpirationInMinute = configuration.GetValue<int>("CacheSettings:AbsoluteExpirationInMinute");
            _slidingExpirationInMinute = configuration.GetValue<int>("CacheSettings:SlidingExpirationInMinute");
        }

        public T? Get<T>(string key) =>
              Get(key) is { } data
                  ? Deserialize<T>(data)
                  : default;

        private byte[]? Get(string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            try
            {
                return _cache.Get(key);
            }
            catch
            {
                return null;
            }
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken token = default) =>

            await GetAsync(key, token) is { } data
                ? Deserialize<T>(data)
                : default;

        private async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            try
            {
                return await _cache.GetAsync(key, token);
            }
            catch
            {
                return null;
            }
        }

        public void Refresh(string key)
        {
            try
            {
                _cache.Refresh(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            try
            {
                await _cache.RefreshAsync(key, token);
                _logger.LogDebug("Cache Refreshed : {0}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public void Remove(string key)
        {
            try
            {
                _cache.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            try
            {
                await _cache.RemoveAsync(key, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public void Set<T>(string key, T value, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null) =>
            Set(key, Serialize(value), slidingExpiration, absoluteExpiration);

        private void Set(string key, byte[] value, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null)
        {
            try
            {
                _cache.Set(key, value, GetOptions(slidingExpiration, absoluteExpiration));
                _logger.LogDebug($"Added to Cache : {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        /// <summary>
        /// Method to storage in the distribute cache the info sent
        /// </summary>
        /// <typeparam name="T">Class type</typeparam>
        /// <param name="key">Key to storage in cache</param>
        /// <param name="value">Data to Caching</param>
        /// <param name="slidingExpiration">the sent is used, otherwise the setting is used and if it is null or equal to zero(0) it sets 30 minutes by default.</param>
        /// <param name="absoluteExpiration">the sent is used, otherwise the setting is used and if it is null or equal to zero(0) it sets 35 minutes by default.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default) =>
            SetAsync(key, Serialize(value), slidingExpiration, absoluteExpiration, cancellationToken);

        private async Task SetAsync(string key, byte[] value, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null, CancellationToken token = default)
        {
            try
            {
                await _cache.SetAsync(key, value, GetOptions(slidingExpiration, absoluteExpiration), token);
                _logger.LogDebug($"Added to Cache : {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private byte[] Serialize<T>(T item) =>
            Encoding.Default.GetBytes(_serializer.ObjectToJson(item!));

        private T Deserialize<T>(byte[] cachedData) =>
            _serializer.JsonToObject<T>(Encoding.Default.GetString(cachedData));

        private DistributedCacheEntryOptions GetOptions(TimeSpan? slidingExpiration, TimeSpan? absoluteExpiration)
        {
            var options = new DistributedCacheEntryOptions();

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
