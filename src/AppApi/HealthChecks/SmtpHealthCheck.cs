using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Shared.Services.Configurations;
using System.Net.Sockets;

namespace AppApi.HealthChecks;

/// <summary>
/// Health check personalizado para verificar conectividad SMTP
/// </summary>
public class SmtpHealthCheck : IHealthCheck
{
    private readonly EMailSettings _emailConfig;
    private readonly ILogger<SmtpHealthCheck> _logger;

    public SmtpHealthCheck(IOptions<EMailSettings> emailConfig, ILogger<SmtpHealthCheck> logger)
    {
        _emailConfig = emailConfig.Value;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar si la configuración está presente
            if (string.IsNullOrEmpty(_emailConfig.Host))
            {
                return HealthCheckResult.Unhealthy(
                    "SMTP configuration is missing",
                    data: new Dictionary<string, object>
                    {
                        { "configured", false }
                    });
            }

            // Intentar conectarse al servidor SMTP
            using var client = new TcpClient();
            await client.ConnectAsync(_emailConfig.Host, _emailConfig.Port, cancellationToken);

            if (client.Connected)
            {
                _logger.LogDebug("SMTP health check successful: {Server}:{Port}", 
                    _emailConfig.Host, _emailConfig.Port);

                return HealthCheckResult.Healthy(
                    "SMTP server is accessible",
                    data: new Dictionary<string, object>
                    {
                        { "server", _emailConfig.Host },
                        { "port", _emailConfig.Port },
                        { "from", _emailConfig.From },
                        { "authenticated", !string.IsNullOrEmpty(_emailConfig.UserName) }
                    });
            }

            return HealthCheckResult.Degraded(
                "SMTP server connection failed",
                data: new Dictionary<string, object>
                {
                    { "server", _emailConfig.Host },
                    { "port", _emailConfig.Port }
                });
        }
        catch (SocketException ex)
        {
            _logger.LogWarning(ex, "SMTP health check failed: {Server}:{Port}", 
                _emailConfig.Host, _emailConfig.Port);

            return HealthCheckResult.Unhealthy(
                $"Cannot connect to SMTP server: {ex.Message}",
                ex,
                data: new Dictionary<string, object>
                {
                    { "server", _emailConfig.Host ?? "not configured" },
                    { "port", _emailConfig.Port },
                    { "error", ex.Message }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP health check encountered an error");

            return HealthCheckResult.Unhealthy(
                $"SMTP health check error: {ex.Message}",
                ex);
        }
    }
}

