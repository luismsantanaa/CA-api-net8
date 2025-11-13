using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Caching;
using Security.Entities.DTOs;
using Shared.Services.Configurations;

namespace AppApi.Configuration
{
    /// <summary>
    /// Validates critical configuration settings at application startup.
    /// Throws exceptions if required configuration is missing or invalid.
    /// </summary>
    public static class ConfigurationValidator
    {
        /// <summary>
        /// Validates all critical configuration sections.
        /// </summary>
        /// <param name="configuration">The application configuration</param>
        /// <param name="logger">Optional logger for validation warnings</param>
        /// <exception cref="InvalidOperationException">Thrown when required configuration is missing or invalid</exception>
        public static void ValidateConfiguration(IConfiguration configuration, ILogger? logger = null)
        {
            var errors = new List<string>();

            // Validate JWT Settings (Critical)
            ValidateJwtSettings(configuration, errors);

            // Validate Connection Strings (Critical - Always using SQL Server)
            ValidateConnectionStrings(configuration, errors);

            // Validate Cache Settings (Conditional)
            ValidateCacheSettings(configuration, errors, logger);

            // Validate Email Settings (Warning only, not critical)
            ValidateEmailSettings(configuration, errors, logger);

            // Validate File Paths (Warning only, not critical)
            ValidateFilePaths(configuration, errors, logger);

            // If there are critical errors, throw exception to prevent application startup
            if (errors.Count > 0)
            {
                var errorMessage = $"Configuration validation failed. Errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}";
                logger?.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            logger?.LogInformation("Configuration validation completed successfully");
        }

        private static void ValidateJwtSettings(IConfiguration configuration, List<string> errors)
        {
            var jwtSection = configuration.GetSection("JwtSettings");
            
            // Key is required
            var key = jwtSection["Key"];
            if (string.IsNullOrWhiteSpace(key))
            {
                errors.Add("JwtSettings:Key is required and cannot be empty");
            }

            // Issuer and Audience are recommended but not strictly required
            // (Already handled in JwtUtils with conditional validation)
        }

        private static void ValidateConnectionStrings(IConfiguration configuration, List<string> errors)
        {
            var connectionStrings = configuration.GetSection("ConnectionStrings");
            
            var appConnection = connectionStrings["ApplicationConnection"];
            if (string.IsNullOrWhiteSpace(appConnection))
            {
                errors.Add("ConnectionStrings:ApplicationConnection is required");
            }

            var identityConnection = connectionStrings["IdentityConnection"];
            if (string.IsNullOrWhiteSpace(identityConnection))
            {
                errors.Add("ConnectionStrings:IdentityConnection is required");
            }
        }

        private static void ValidateCacheSettings(IConfiguration configuration, List<string> errors, ILogger? logger)
        {
            var cacheSection = configuration.GetSection("CacheSettings");
            var useDistributedCache = cacheSection.GetValue<bool>("UseDistributedCache");
            var preferRedis = cacheSection.GetValue<bool>("PreferRedis");
            
            if (useDistributedCache && preferRedis)
            {
                var redisUrl = cacheSection["RedisURL"];
                if (string.IsNullOrWhiteSpace(redisUrl))
                {
                    errors.Add("CacheSettings:RedisURL is required when UseDistributedCache=true and PreferRedis=true");
                }
                else
                {
                    // Basic validation: should contain : for host:port format
                    if (!redisUrl.Contains(':'))
                    {
                        logger?.LogWarning("CacheSettings:RedisURL format may be invalid. Expected format: host:port");
                    }
                }
            }
        }

        private static void ValidateEmailSettings(IConfiguration configuration, List<string> errors, ILogger? logger)
        {
            var emailSection = configuration.GetSection("EMailSettings");
            var from = emailSection["From"];
            var host = emailSection["Host"];
            var port = emailSection["Port"];
            var userName = emailSection["UserName"];
            var password = emailSection["Password"];

            // Email settings are optional, but if one is provided, others should be too
            var hasAnyEmailSetting = !string.IsNullOrWhiteSpace(from) || 
                                     !string.IsNullOrWhiteSpace(host) || 
                                     !string.IsNullOrWhiteSpace(userName);

            if (hasAnyEmailSetting)
            {
                if (string.IsNullOrWhiteSpace(from))
                {
                    logger?.LogWarning("EMailSettings:From is missing but other email settings are configured");
                }
                if (string.IsNullOrWhiteSpace(host))
                {
                    logger?.LogWarning("EMailSettings:Host is missing but other email settings are configured");
                }
                if (string.IsNullOrWhiteSpace(userName))
                {
                    logger?.LogWarning("EMailSettings:UserName is missing but other email settings are configured");
                }
                if (string.IsNullOrWhiteSpace(password))
                {
                    logger?.LogWarning("EMailSettings:Password is missing but other email settings are configured");
                }
                if (string.IsNullOrWhiteSpace(port))
                {
                    logger?.LogWarning("EMailSettings:Port is missing but other email settings are configured");
                }
            }
        }

        private static void ValidateFilePaths(IConfiguration configuration, List<string> errors, ILogger? logger)
        {
            var filePathsSection = configuration.GetSection("FilesPaths");
            var testPath = filePathsSection["TestPath"];

            // File paths are optional, but if configured, validate accessibility in production
            if (!string.IsNullOrWhiteSpace(testPath))
            {
                // In production, we might want to check if the path exists or is accessible
                // For now, just log a warning if it looks like a placeholder
                if (testPath.Contains("YOUR_SERVER") || testPath.Contains("YOUR_PATH"))
                {
                    logger?.LogWarning("FilesPaths:TestPath appears to be a placeholder value");
                }
            }
        }
    }
}

