using Domain.Entities.Examples;
using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Contracts
{
    public interface IApplicationDbContext
    {
        EntityEntry Entry(object entity);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        DatabaseFacade database { get; }

        #region Region to Put all DdSet Use   
        // Example
        DbSet<TestCategory> TestCategories { get; set; }
        DbSet<TestProduct> TestProducts { get; set; }
        // Shared
        DbSet<AuditLog> AuditLogs { get; set; }
        DbSet<MailNotificationTemplate> MailNotifications { get; set; }
        DbSet<UploadedFile> UploadedFiles { get; set; }
        //***** Application Dbo *****

        #endregion
    }
}
