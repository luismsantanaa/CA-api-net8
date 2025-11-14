using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;

namespace AppApi.HealthChecks;

/// <summary>
/// Health check general de la aplicación
/// Verifica información básica de la aplicación y su estado
/// </summary>
public class ApplicationHealthCheck : IHealthCheck
{
    private readonly ILogger<ApplicationHealthCheck> _logger;
    private static readonly DateTime _startTime = DateTime.UtcNow;

    public ApplicationHealthCheck(ILogger<ApplicationHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString() ?? "unknown";
            var uptime = DateTime.UtcNow - _startTime;

            var data = new Dictionary<string, object>
            {
                { "version", version },
                { "uptime_seconds", (int)uptime.TotalSeconds },
                { "uptime_formatted", $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m" },
                { "start_time", _startTime },
                { "environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown" },
                { "dotnet_version", Environment.Version.ToString() },
                { "os", Environment.OSVersion.ToString() },
                { "machine_name", Environment.MachineName },
                { "processor_count", Environment.ProcessorCount },
                { "working_set_mb", (Environment.WorkingSet / 1024 / 1024) },
            };

            _logger.LogDebug("Application health check successful. Uptime: {Uptime}", uptime);

            return Task.FromResult(HealthCheckResult.Healthy(
                "Application is running",
                data: data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application health check encountered an error");

            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Application health check error: {ex.Message}",
                ex));
        }
    }
}

