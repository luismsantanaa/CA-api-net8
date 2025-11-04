using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.DbContexts;
using Persistence.Seeds.Examples;
using Shared.Services.Contracts;

namespace Persistence.Seeds
{
    public class ApplicationSeedData
    {
        private readonly ApplicationDbContext? _context;
        private readonly ILogger? _logger;
        private readonly IJsonService _serializerService;

        public ApplicationSeedData(ApplicationDbContext context, ILoggerFactory? loggerFactory, IJsonService serializerService)
        {
            _context = context;
            _logger = loggerFactory!.CreateLogger<ApplicationSeedData>();
            _serializerService = serializerService;
        }

        public async Task UploadExampleData()
        {
            _logger!.LogInformation("Validating if data exist.");
            if (_context!.TestProducts!.Any() && _context.TestCategories!.Any()) return;

            var isInMemory = Microsoft.EntityFrameworkCore.InMemoryDatabaseFacadeExtensions.IsInMemory(_context.Database);

            if (isInMemory)
            {
                // Para InMemory Database: No usar transacciones
                await UploadExampleDataWithoutTransaction();
            }
            else
            {
                // Para SQL Server: Usar transacciones
                await UploadExampleDataWithTransaction();
            }
        }

        private async Task UploadExampleDataWithoutTransaction()
        {
            try
            {
                _logger!.LogInformation("Loading Category seed data (InMemory mode).");

                var categoryData = new Categories().CategoriesList!.ToList();
                await _context!.TestCategories.AddRangeAsync(categoryData);
                await _context.SaveChangesAsync(CancellationToken.None);

                _logger!.LogInformation("Category seed data uploaded to database.");

                _logger!.LogInformation("Loading Product seed data.");

                var productData = new Products().ProductsList!.ToList();
                Guid[] guids = categoryData.Select(x => x.Id).ToArray();
                var random = new Random();
                foreach (var item in productData)
                {
                    var index = random.Next(1, categoryData.Count);
                    item.CategoryId = (Guid)guids[index]!;
                }
                await _context.TestProducts.AddRangeAsync(productData);
                await _context.SaveChangesAsync(CancellationToken.None);

                _logger!.LogInformation("Product seed data uploaded to database.");
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "Error uploading example data: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private async Task UploadExampleDataWithTransaction()
        {
            _logger!.LogInformation("Init Data-Base Transaction.");
            var strategy = _context!.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = _context!.Database.BeginTransaction();
                try
                {
                    _logger!.LogInformation("Validating if data exist.");

                    _logger!.LogInformation("Load Category seed data.");

                    var categoryData = new Categories().CategoriesList!.ToList();
                    await _context.TestCategories.AddRangeAsync(categoryData);
                    await _context.SaveChangesAsync(CancellationToken.None);

                    _logger!.LogInformation("Category seed data Upload to Data-Base.");

                    _logger!.LogInformation("Load Product seed data.");

                    var productData = new Products().ProductsList!.ToList();
                    Guid[] guids = categoryData.Select(x => x.Id).ToArray();
                    var random = new Random();
                    foreach (var item in productData)
                    {
                        var index = random.Next(1, categoryData.Count);
                        item.CategoryId = (Guid)guids[index]!;
                    }
                    await _context.TestProducts.AddRangeAsync(productData);
                    await _context.SaveChangesAsync(CancellationToken.None);

                    _logger!.LogInformation("Product seed data Upload to Data-Base.");

                    await transaction.CommitAsync();

                    _logger!.LogInformation("Data-Base Transaction Complete.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger!.LogError(ex, ex.Message);
                    throw;
                }
            });
        }

        public async Task UploadSharedData()
        {
            _logger!.LogInformation("Validating if data exist.");
            if (_context!.MailNotifications.Any()) return;

            var isInMemory = Microsoft.EntityFrameworkCore.InMemoryDatabaseFacadeExtensions.IsInMemory(_context.Database);

            if (isInMemory)
            {
                // Para InMemory Database: No usar transacciones
                await UploadSharedDataWithoutTransaction();
            }
            else
            {
                // Para SQL Server: Usar transacciones
                await UploadSharedDataWithTransaction();
            }
        }

        private async Task UploadSharedDataWithoutTransaction()
        {
            try
            {
                _logger!.LogInformation("Load MailNotification seed data (InMemory mode).");

                var mailTemplateData = new MailNotificationData().GetMailNotificationTemplate();
                await _context!.MailNotifications.AddAsync(mailTemplateData!);
                await _context.SaveChangesAsync(CancellationToken.None);

                _logger!.LogInformation("MailNotification seed data uploaded to database.");
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "Error uploading shared data: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private async Task UploadSharedDataWithTransaction()
        {
            _logger!.LogInformation("Init Data-Base Transaction.");
            using var transaction = _context!.Database.BeginTransaction();
            try
            {
                _logger!.LogInformation("Load MailNotification seed data.");

                var mailTemplateData = new MailNotificationData().GetMailNotificationTemplate();
                await _context.MailNotifications.AddAsync(mailTemplateData!);
                await _context.SaveChangesAsync(CancellationToken.None);

                _logger!.LogInformation("MailNotification seed data Upload to Data-Base.");

                await transaction.CommitAsync();
                _logger!.LogInformation("Data-Base Transaction Complete.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger!.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
