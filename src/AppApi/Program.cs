using AppApi.Extensions;
using AppApi.Middleware;
using Application;
using Persistence;
using Security;
using Security.DbContext;
using Security.Entities.DTOs;
using Serilog;
using Shared;
using Shared.Services.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<EMailSettings>(builder.Configuration.GetSection("EMailSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Validate critical configuration settings before proceeding
// This ensures the application fails fast with clear error messages if configuration is invalid
try
{
    // Create a temporary logger factory for configuration validation
    using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    var logger = loggerFactory.CreateLogger<Program>();

    AppApi.Configuration.ConfigurationValidator.ValidateConfiguration(builder.Configuration, logger);
}
catch (InvalidOperationException ex)
{
    // Log error to console before throwing (Serilog may not be initialized yet)
    Console.Error.WriteLine($"FATAL: {ex.Message}");
    throw;
}

builder.Services.AddEssentials(builder.Configuration);
builder.Services.AddContextToPersistence(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddSharedServices(builder.Configuration);
builder.Services.AddSecurityServices(builder.Configuration);

// Configure Advanced Health Checks
builder.Services.AddAdvancedHealthChecks(builder.Configuration);

// CORS policy per environment
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
        else
        {
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
            }
            else
            {
                // default deny-all if not configured
                policy.WithOrigins(Array.Empty<string>());
            }
        }
    });
});

// Crear carpeta de logs si no existe
var logPath = Path.Combine(Directory.GetCurrentDirectory(), "log");
if (!Directory.Exists(logPath))
{
    Directory.CreateDirectory(logPath);
}

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

app.UseCors("CorsPolicy");

// Add Correlation ID middleware early in pipeline for request tracing
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSerilogRequestLogging();

// ConfigureSwagger now handles environment checking internally
app.ConfigureSwagger();

app.UseMiddleware<ExceptionMiddleware>();

// Apply JwtMiddleware only if UseCustomAuthorization is enabled
// This middleware is used for custom JWT validation when not using standard ASP.NET Core authentication
var useCustomAuthorization = builder.Configuration.GetValue<bool>("AuthorizationSettings:UseCustomAuthorization", false);
if (useCustomAuthorization)
{
    app.UseMiddleware<JwtMiddleware>();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// *** NORMAL AUTHENTICATION  ***
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Configure Advanced Health Check Endpoints
app.MapAdvancedHealthChecks();

#region Seed Data
// NOTA: La estructura de base de datos se maneja desde el proyecto SQL Server Database
// No usamos migraciones de EF Core debido al bug conocido en EF Core 9
var runSeeds = builder.Configuration.GetValue<bool>("DatabaseOptions:RunSeedsOnStartup");

using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider;
    var loggerFactory = service.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Program>();

    try
    {
        if (runSeeds)
        {
            logger.LogInformation("Verificando y cargando datos iniciales...");

            // Seed de usuario de prueba para Identity
            var userManager = service.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser>>();
            var identityContext = service.GetRequiredService<IdentityContext>();
            var identitySeed = new Security.Seeds.IdentitySeedData(identityContext, userManager, loggerFactory);
            await identitySeed.SeedTestUser();

            logger.LogInformation("Datos iniciales completados.");
            logger.LogInformation("Usuario de prueba: test@mardom.com / Test123!@#");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error durante la carga de datos iniciales");
        throw;
    }
}
#endregion

app.Run();
