using Domain.Base;
using Domain.Entities.Examples;
using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Persistence.Constants;
using Persistence.DbContexts.Contracts;
using Persistence.EntitiesConfigurations.Shared;
using Persistence.InternalModels;
using Shared.Services.Contracts;

namespace Persistence.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        private Guid? _user;
        private readonly DateTime _localTime;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
                  IGetUserServices userService,
                  ILocalTimeService localTimeService) : base(options)
        {
            var usr = userService.UserId;

            _user = usr;
            _localTime = localTimeService.LocalTime;
#if DEBUG
            _user = _user.HasValue ? _user : Guid.NewGuid();
#endif
        }

        public DatabaseFacade database => base.Database;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG            
            optionsBuilder.EnableSensitiveDataLogging(true);
#endif          
            base.OnConfiguring(optionsBuilder);
        }

        #region Entities   
        // Examples
        public DbSet<TestCategory> TestCategories { get; set; }
        public DbSet<TestProduct> TestProducts { get; set; }
        // Shared
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<MailNotificationTemplate> MailNotifications { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
        //***** Application *****\\
        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            //********************* Configurations *********************\\           
            // Shared            
            builder.ApplyConfiguration(new MailNotificationTemplateConfigurations());
            builder.ApplyConfiguration(new UploadedFileConfigurations());
            //
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedOn = _localTime;
                        entry.Entity.CreatedBy = _user.ToString()!;
                        entry.Entity.Version = 1;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedOn = _localTime;
                        entry.Entity.LastModifiedBy = _user.ToString()!;
                        entry.Entity.Version++;
                        break;

                    case EntityState.Deleted:
                        if (entry.Entity is SoftDelete softDelete)
                        {
                            softDelete.DeletedBy = _user.ToString()!;
                            softDelete.DeletedAt = _localTime;
                            softDelete.IsDeleted = true;
                            softDelete.Version++;
                            entry.State = EntityState.Modified;
                        }
                        break;
                }
            }

            foreach (var entry in ChangeTracker.Entries<TraceableEntity>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Detached:
                    case EntityState.Deleted:
                    case EntityState.Modified:
                        await OnBeforeSaveChanges(entry);
                        break;
                    default:
                        break;
                }
            }

            int result;
            if (_user == null)
            {
                result = await base.SaveChangesAsync(cancellationToken);
            }
            else
            {
                result = await base.SaveChangesAsync(CancellationToken.None);
            }

            return result;
        }

        private async Task OnBeforeSaveChanges(EntityEntry entry)
        {
            ChangeTracker.DetectChanges();

            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                return;

            var auditEntry = new AuditEntry(entry, _localTime)
            {
                UserId = (Guid)_user!,
                TableName = entry.Entity.GetType().Name,
                AuditType = AuditType.None
            };

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues![propertyName] = property.CurrentValue!;
                    continue;
                }
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.AuditType = AuditType.Create;
                        auditEntry.NewValues![propertyName] = property.CurrentValue!;
                        break;

                    case EntityState.Deleted:
                        auditEntry.AuditType = AuditType.Delete;
                        auditEntry.OldValues![propertyName] = property.OriginalValue!;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.ChangedColumns!.Add(propertyName);
                            auditEntry.AuditType = AuditType.Update;
                            auditEntry.OldValues![propertyName] = property.OriginalValue!;
                            auditEntry.NewValues![propertyName] = property.CurrentValue!;
                        }
                        break;
                }
            }
            await AuditLogs.AddAsync(auditEntry.ToAudit());
        }
    }
}
