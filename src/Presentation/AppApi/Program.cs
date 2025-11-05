using AppApi.Extensions;
using AppApi.Middleware;
using Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.DbContexts;
using Persistence.Seeds;
using Security;
using Security.DbContext;
using Security.Entities.DTOs;
using Security.Seeds;
using Serilog;
using Shared;
using Shared.Services.Configurations;
using Shared.Services.Contracts;

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

// Configure Health Checks
app.ConfigureHealthChecks();

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

#region Migration and Seed Data
var runMigrations = builder.Configuration.GetValue<bool>("DatabaseOptions:RunMigrationsOnStartup");
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider;
    var loggerFactory = service.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Program>();

    try
    {
        var context = service.GetRequiredService<ApplicationDbContext>();
        var isInMemory = Microsoft.EntityFrameworkCore.InMemoryDatabaseFacadeExtensions.IsInMemory(context.Database);

        if (isInMemory)
        {
            // Para InMemory: Solo cargar datos de seed, no migraciones
            logger.LogInformation("Usando base de datos en memoria. Cargando datos iniciales...");
            var serializerService = service.GetRequiredService<IJsonService>();
            var seedData = new ApplicationSeedData(context, loggerFactory, serializerService);
            await seedData.UploadExampleData();
            await seedData.UploadSharedData();
            logger.LogInformation("Datos iniciales cargados exitosamente en la base de datos en memoria.");

            // Cargar datos de Identity (usuario de prueba)
            var identityContext = service.GetRequiredService<IdentityContext>();
            var userManager = service.GetRequiredService<UserManager<IdentityUser>>();
            if (identityContext != null && userManager != null)
            {
                logger.LogInformation("Cargando usuario de prueba en IdentityContext...");
                var identitySeedData = new IdentitySeedData(identityContext, userManager, loggerFactory);
                await identitySeedData.SeedTestUser();
                logger.LogInformation("Usuario de prueba cargado exitosamente.");
            }
        }
        else if (runMigrations)
        {
            // Para SQL Server: Ejecutar migraciones y luego seed
            logger.LogInformation("Ejecutando migraciones de base de datos...");
            await context.Database.MigrateAsync();

            var serializerService = service.GetRequiredService<IJsonService>();
            var seedData = new ApplicationSeedData(context, loggerFactory, serializerService);
            await seedData.UploadExampleData();
            await seedData.UploadSharedData();

            var contextIdentity = service.GetRequiredService<IdentityContext>();
            await contextIdentity.Database.MigrateAsync();

            logger.LogInformation("Migraciones y datos iniciales completados.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error durante la inicialización de la base de datos o carga de datos iniciales");
        throw; // Relanzar para que la aplicación no inicie con datos corruptos
    }
}
#endregion

app.Run();
