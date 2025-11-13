using Microsoft.Extensions.Logging;
using Persistence.Caching.Contracts;
using Shared.Exceptions;

namespace Persistence.Caching.Extensions
{
    public static class CacheServiceExtensions
    {
        /// <summary>
        /// Gets a value from cache, or sets it using the provided callback if not found.
        /// Improved exception handling to preserve specific error types and provide better diagnostics.
        /// </summary>
        public static async Task<T?> GetOrSetAsync<T>(
            this ICacheService cache, 
            string key, 
            Func<Task<T>> getItemCallback, 
            TimeSpan? slidingExpiration = null, 
            CancellationToken cancellationToken = default,
            ILogger? logger = null)
        {
            try
            {
                T? value = await cache.GetAsync<T>(key, cancellationToken);

                if (value is not null)
                {
                    logger?.LogDebug("Cache hit for key: {Key}", key);
                    return value;
                }

                logger?.LogDebug("Cache miss for key: {Key}, fetching from source", key);

                value = await getItemCallback();

                if (value is not null)
                {
                    await cache.SetAsync(key, value, slidingExpiration, null, cancellationToken);
                    logger?.LogDebug("Value cached for key: {Key}", key);
                }

                return value;
            }
            catch (InternalServerError)
            {
                // Re-throw InternalServerError as-is to preserve original exception type
                throw;
            }
            catch (BadRequestException)
            {
                // Re-throw BadRequestException as-is to preserve original exception type
                throw;
            }
            catch (NotFoundException)
            {
                // Re-throw NotFoundException as-is to preserve original exception type
                throw;
            }
            catch (ValidationException)
            {
                // Re-throw ValidationException as-is to preserve original exception type
                throw;
            }
            catch (Exception ex)
            {
                // Log the error with context for diagnostics
                logger?.LogError(ex, 
                    "Unexpected error in GetOrSetAsync for cache key: {Key}. Error: {ErrorMessage}", 
                    key, ex.Message);

                // Only wrap generic exceptions, preserving inner exception details
                var message = ex.InnerException?.Message ?? ex.Message;
                throw new InternalServerError($"Cache operation failed for key '{key}': {message}", ex);
            }
        }
    }
}
