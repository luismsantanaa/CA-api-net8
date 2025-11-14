using AppApi.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppApi.Extensions;

/// <summary>
/// Extensiones para configurar Health Checks avanzados
/// </summary>
public static class HealthCheckExtensions
{
    public static IServiceCollection AddAdvancedHealthChecks(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // ============================================
        // 1. Application Health Check
        // ============================================
        healthChecksBuilder.AddCheck<ApplicationHealthCheck>(
            name: "application",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "ready", "application" });

        // ============================================
        // 2. SQL Server - Application Database
        // ============================================
        var appConnectionString = configuration.GetConnectionString("ApplicationConnection");
        if (!string.IsNullOrEmpty(appConnectionString))
        {
            healthChecksBuilder.AddSqlServer(
                connectionString: appConnectionString,
                healthQuery: "SELECT 1;",
                name: "sql_application",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "ready", "database", "sql" },
                timeout: TimeSpan.FromSeconds(5));
        }

        // ============================================
        // 3. SQL Server - Identity Database
        // ============================================
        var identityConnectionString = configuration.GetConnectionString("IdentityConnection");
        if (!string.IsNullOrEmpty(identityConnectionString) && 
            identityConnectionString != appConnectionString)
        {
            healthChecksBuilder.AddSqlServer(
                connectionString: identityConnectionString,
                healthQuery: "SELECT 1;",
                name: "sql_identity",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "ready", "database", "sql", "identity" },
                timeout: TimeSpan.FromSeconds(5));
        }

        // ============================================
        // 4. Redis (opcional - solo si está configurado)
        // ============================================
        var redisConnectionString = configuration.GetConnectionString("RedisConnection");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            healthChecksBuilder.AddRedis(
                redisConnectionString,
                name: "redis",
                failureStatus: HealthStatus.Degraded, // No crítico
                tags: new[] { "cache", "redis" },
                timeout: TimeSpan.FromSeconds(3));
        }

        // ============================================
        // 5. SMTP Email Service
        // ============================================
        healthChecksBuilder.AddCheck<SmtpHealthCheck>(
            name: "smtp",
            failureStatus: HealthStatus.Degraded, // No crítico para la app
            tags: new[] { "email", "smtp" });

        // ============================================
        // 6. Health Checks UI (Dashboard)
        // ============================================
        services.AddHealthChecksUI(setupSettings: setup =>
        {
            setup.AddHealthCheckEndpoint(
                name: "Clean Architecture API",
                uri: "/health");
            
            // Configurar evaluación cada 30 segundos
            setup.SetEvaluationTimeInSeconds(30);
            
            // Mantener historial de 24 horas
            setup.MaximumHistoryEntriesPerEndpoint(50);
        })
        .AddInMemoryStorage();

        return services;
    }

    public static IEndpointRouteBuilder MapAdvancedHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        // ============================================
        // Endpoint principal de Health Check (JSON detallado)
        // ============================================
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        // ============================================
        // Endpoint simple de Health Check (para load balancers)
        // ============================================
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            AllowCachingResponses = false,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        // ============================================
        // Endpoint de liveness (para Kubernetes)
        // ============================================
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Name == "application",
            AllowCachingResponses = false
        });

        // ============================================
        // Health Checks UI Dashboard
        // ============================================
        endpoints.MapHealthChecksUI(setup =>
        {
            setup.UIPath = "/health-ui";
            setup.ApiPath = "/health-ui-api";
        });

        return endpoints;
    }
}

