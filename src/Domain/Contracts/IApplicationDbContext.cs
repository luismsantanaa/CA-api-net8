using Domain.Entities.Examples;
using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Domain.Contracts
{
    public interface IApplicationDbContext
    {
        EntityEntry Entry(object entity);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        DatabaseFacade database { get; }

        #region DbSets   
        // Shared Entities
        DbSet<AuditLog> AuditLogs { get; set; }
        DbSet<MailNotificationTemplate> MailNotifications { get; set; }
        DbSet<UploadedFile> UploadedFiles { get; set; }

        // Example Entities
        DbSet<TestCategory> TestCategories { get; set; }
        DbSet<TestProduct> TestProducts { get; set; }

        //***** Application DbSets *****
        // Add your application-specific DbSets here

        #endregion
    }
}

