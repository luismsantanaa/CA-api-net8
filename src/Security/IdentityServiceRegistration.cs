using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Security.DbContext;
using Security.Entities.DTOs;
using Security.Services.Concrete;
using Security.Services.Contracts;

namespace Security
{
    public static class IdentityServiceRegistration
    {
        public static void AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

            // ActiveDirectoryService is Windows-specific
            if (OperatingSystem.IsWindows())
            {
                services.AddTransient<IActiveDirectoryService, ActiveDirectoryService>();
            }

            services.AddTransient<IAppAuthService, AppAuthService>();
            services.AddTransient<IMardomAuthService, MardomAuthService>();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JwtSettings:Key"]!)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                RequireExpirationTime = false,
                ClockSkew = TimeSpan.Zero,
            };

            services.AddSingleton(tokenValidationParameters);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = tokenValidationParameters;

                // Enable detailed error information for debugging
                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler>>();
                        logger.LogError(context.Exception, "JWT Authentication failed");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler>>();
                        logger.LogInformation("JWT Token validated successfully for user: {User}", context.Principal?.Identity?.Name);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler>>();
                        logger.LogWarning("JWT Challenge: {Error}, {ErrorDescription}", context.Error, context.ErrorDescription);
                        return Task.CompletedTask;
                    }
                };
            });

            // TODO: Consider adding authorization policies based on roles/claims if the domain requires it.
            // Example:
            // services.AddAuthorization(options =>
            // {
            //     options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            //     options.AddPolicy("RequireEmailVerified", policy => policy.RequireClaim("EmailVerified", "true"));
            // });
        }
    }
}
