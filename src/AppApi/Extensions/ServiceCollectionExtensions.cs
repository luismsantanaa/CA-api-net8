using AppApi.Authorization;
using AppApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Persistence.Caching;
using Persistence.DbContexts;
using Persistence.DbContexts.Contracts;
using Security.DbContext;

namespace AppApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEssentials(this IServiceCollection services, IConfiguration configuration)
        {
            services.RegisterSwagger();
            services.RegisterHealthChecks(configuration);
            services.AddScoped<IGetUserServices, GetUserService>();
            services.AddScoped<IJwtUtils, JwtUtils>();
        }

        private static void RegisterSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(option =>
            {
                option.EnableAnnotations();
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "MARDOM - Template.", Version = "v1", Description = "API Template to generate new projects." });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });
        }

        public static void AddContextToPersistence(this IServiceCollection services, IConfiguration configuration)
        {

            _ = services.AddDbContext<IdentityContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("IdentityConnection"),
                sqlServerOptionsAction =>
                {
                    sqlServerOptionsAction.CommandTimeout(int.Parse(configuration["DatabaseOptions:CommandTimeout"]!));
                    sqlServerOptionsAction.EnableRetryOnFailure(int.Parse(configuration["DatabaseOptions:MaxRetryCount"]!));
                    sqlServerOptionsAction.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName);
                });
                options.EnableDetailedErrors(bool.Parse(configuration["DatabaseOptions:EnableDetailedErrors"]!));
            });

            _ = services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("ApplicationConnection"), sqlServerOptionsAction =>
                {
                    sqlServerOptionsAction.CommandTimeout(int.Parse(configuration["DatabaseOptions:CommandTimeout"]!));
                    sqlServerOptionsAction.EnableRetryOnFailure(int.Parse(configuration["DatabaseOptions:MaxRetryCount"]!));
                    sqlServerOptionsAction.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                });
                options.EnableDetailedErrors(bool.Parse(configuration["DatabaseOptions:EnableDetailedErrors"]!));
            });
        }

        private static void RegisterHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var healthChecks = services.AddHealthChecks();

            var applicationConnection = configuration.GetConnectionString("ApplicationConnection");
            var identityConnection = configuration.GetConnectionString("IdentityConnection");

            if (!string.IsNullOrWhiteSpace(applicationConnection))
            {
                healthChecks.AddSqlServer(
                    applicationConnection,
                    name: "application-db",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "db", "sql", "application" });
            }

            if (!string.IsNullOrWhiteSpace(identityConnection))
            {
                healthChecks.AddSqlServer(
                    identityConnection,
                    name: "identity-db",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "db", "sql", "identity" });
            }

            // Redis Health Check (only if Redis is preferred and configured)
            var cacheSettings = configuration.GetSection(nameof(CacheSettings)).Get<CacheSettings>();
            if (cacheSettings != null && cacheSettings.UseDistributedCache &&
                cacheSettings.PreferRedis && !string.IsNullOrWhiteSpace(cacheSettings.RedisURL))
            {
                healthChecks.AddRedis(
                    cacheSettings.RedisURL,
                    name: "redis",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "cache", "redis" });
            }

            // Health Checks UI - Registered always, but only enabled in Development via middleware
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(10);
                setup.MaximumHistoryEntriesPerEndpoint(50);
                setup.AddHealthCheckEndpoint("API", "/health");
            }).AddInMemoryStorage();
        }
    }
}
