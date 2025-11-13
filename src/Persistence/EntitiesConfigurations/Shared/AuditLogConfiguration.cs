using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Constants;

namespace Persistence.EntitiesConfigurations.Shared
{
    internal class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs", SchemasDb.Shared);

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UserId)
                .IsRequired()
                .HasColumnType("uniqueidentifier");

            builder.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnType("varchar(10)");

            builder.Property(e => e.TableName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            builder.Property(e => e.DateTime)
                .IsRequired()
                .HasColumnType("DateTime");

            builder.Property(e => e.OldValues)
                .HasColumnType("varchar(max)");

            builder.Property(e => e.NewValues)
                .HasColumnType("varchar(max)");

            builder.Property(e => e.AffectedColumns)
                .HasColumnType("varchar(max)");

            builder.Property(e => e.PrimaryKey)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");
        }
    }
}

