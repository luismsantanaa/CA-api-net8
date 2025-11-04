using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Caching;
using Persistence.Caching.Contracts;
using Persistence.Repositories;
using Persistence.Repositories.Application;
using Persistence.Repositories.Contracts;

namespace Persistence
{
    public static class PersistenceServicesRegistration
    {
        public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Unit of Work pattern - Scoped ensures one instance per HTTP request
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Repository Factory and repositories remain for backward compatibility
            // New code should use IUnitOfWork.Repository<TEntity>() instead
            services.AddScoped<IRepositoryFactory, RepositoryFactory>();
            services.AddTransient(typeof(IGenericRepository<>), typeof(ApplicationRepository<>));

            #region Cache Service Settings
            var settings = configuration.GetSection(nameof(CacheSettings)).Get<CacheSettings>();

            if (settings!.UseDistributedCache)
            {
                services.AddScoped<ICacheKeyService, CacheKeyService>();

                var useRedis = settings.PreferRedis && !string.IsNullOrWhiteSpace(settings.RedisURL);
                if (useRedis)
                {
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = settings.RedisURL;
                        options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions()
                        {
                            AbortOnConnectFail = false,
                            EndPoints = { settings.RedisURL! }
                        };
                    });
                }
                else
                {
                    // Fallback to in-memory distributed cache
                    services.AddDistributedMemoryCache();
                }

                services.AddTransient<ICacheService, DistributedCacheService>();
            }
            else
            {
                services.AddMemoryCache();
                services.AddScoped<ICacheKeyService, CacheKeyService>();
                services.AddTransient<ICacheService, LocalCacheService>();
            }
            #endregion

            //Custom Repositories

        }
    }
}
