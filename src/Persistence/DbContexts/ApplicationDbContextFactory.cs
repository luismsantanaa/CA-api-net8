using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Persistence.DbContexts.Contracts;
using Shared.Services.Contracts;

namespace Persistence.DbContexts
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            Console.WriteLine("[Factory] Current Directory: " + Directory.GetCurrentDirectory());

            // Crear servicios mock para el diseño
            var mockUserService = new DesignTimeGetUserServices();
            var mockLocalTimeService = new DesignTimeLocalTimeService();

            // Buscar appsettings.json en la ubicación del proyecto API
            string basePath = Directory.GetCurrentDirectory();
            string appSettingsPath = Path.Combine(basePath, "appsettings.json");

            Console.WriteLine($"[Factory] Trying: {appSettingsPath}");

            // Si no está en el directorio actual, buscar en el proyecto AppApi
            if (!File.Exists(appSettingsPath))
            {
                var parentDir = Directory.GetParent(basePath);
                if (parentDir != null)
                {
                    // Intentar ruta relativa desde Persistence hacia AppApi
                    appSettingsPath = Path.Combine(parentDir.FullName, "Presentation", "AppApi", "appsettings.json");
                    Console.WriteLine($"[Factory] Trying: {appSettingsPath}");
                }
            }

            if (File.Exists(appSettingsPath))
            {
                basePath = Path.GetDirectoryName(appSettingsPath)!;
                Console.WriteLine($"[Factory] Found appsettings.json at: {basePath}");
            }

            // Configuración
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            Console.WriteLine($"[Factory] Using connection string from: {basePath}");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Usar SQL Server para migraciones (las entidades problemáticas ya están renombradas)
            Console.WriteLine("[Factory] Configuring SQL Server...");
            var connectionString = configuration.GetConnectionString("ApplicationConnection");
            Console.WriteLine($"[Factory] Connection string (first 50 chars): {connectionString?.Substring(0, Math.Min(50, connectionString?.Length ?? 0))}...");

            optionsBuilder.UseSqlServer(connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));
            Console.WriteLine($"[Factory] Migrations assembly: {typeof(ApplicationDbContext).Assembly.GetName().Name}");

            Console.WriteLine("[Factory] Creating ApplicationDbContext with mock services...");
            var context = new ApplicationDbContext(
                optionsBuilder.Options,
                mockUserService,
                mockLocalTimeService);

            Console.WriteLine("[Factory] ApplicationDbContext created successfully!");
            return context;
        }
    }

    // Servicios mock para tiempo de diseño
    internal class DesignTimeGetUserServices : IGetUserServices
    {
        public bool? IsAuthenticated => false;
        public Guid? UserId => Guid.NewGuid();
    }

    internal class DesignTimeLocalTimeService : ILocalTimeService
    {
        public DateTime LocalTime => DateTime.Now;
    }
}

