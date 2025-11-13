using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Security.DbContext;
using Security.Entities;

namespace Security.Seeds
{
    public class IdentitySeedData
    {
        private readonly IdentityContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger _logger;

        public IdentitySeedData(
            IdentityContext context,
            UserManager<IdentityUser> userManager,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<IdentitySeedData>();
        }

        public async Task SeedTestUser()
        {
            try
            {
                _logger.LogInformation("Validando si el usuario de prueba ya existe...");

                // Verificar si el usuario ya existe
                var existingUser = await _userManager.FindByEmailAsync("test@mardom.com");
                if (existingUser != null)
                {
                    _logger.LogInformation("El usuario de prueba ya existe. Saltando creación.");
                    return;
                }

                _logger.LogInformation("Creando usuario de prueba...");

                // Crear IdentityUser
                var testUser = new IdentityUser
                {
                    UserName = "testuser",
                    Email = "test@mardom.com",
                    EmailConfirmed = true,
                    LockoutEnabled = false
                };

                // Crear usuario con contraseña
                var password = "Test123!@#"; // Contraseña de prueba
                var result = await _userManager.CreateAsync(testUser, password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Error al crear usuario de prueba: {Errors}", errors);
                    throw new Exception($"Error al crear usuario de prueba: {errors}");
                }

                _logger.LogInformation("IdentityUser creado exitosamente. ID: {UserId}", testUser.Id);

                // Crear AppUser asociado
                var appUser = new AppUser
                {
                    UserId = testUser.Id,
                    Codigo = "00000001",
                    FullName = "Usuario de Prueba",
                    Email = "test@mardom.com",
                    Department = "Desarrollo",
                    Position = "Desarrollador",
                    Company = "Maritima Dominicana S.A.S.",
                    Office = "Oficina Principal"
                };

                _context.AppUsers?.Add(appUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuario de prueba creado exitosamente.");
                _logger.LogInformation("Credenciales:");
                _logger.LogInformation("  Email: test@mardom.com");
                _logger.LogInformation("  Username: testuser");
                _logger.LogInformation("  Password: Test123!@#");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario de prueba: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}

