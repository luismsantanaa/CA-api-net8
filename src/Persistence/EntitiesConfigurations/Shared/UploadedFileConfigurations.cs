using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Constants;

namespace Persistence.EntitiesConfigurations.Shared
{
    internal class UploadedFileConfigurations : IEntityTypeConfiguration<UploadedFile>
    {
        public void Configure(EntityTypeBuilder<UploadedFile> builder)
        {
            builder.ToTable("UploadedFiles", SchemasDb.Shared);

            builder.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(250)
                   .IsUnicode(false);

            builder.Property(e => e.Type)
                  .IsRequired()
                  .HasMaxLength(50)
                  .IsUnicode(false);

            builder.Property(e => e.Extension)
                  .IsRequired()
                  .HasMaxLength(6)
                  .IsUnicode(false);

            builder.Property(e => e.Size)
                  .HasPrecision(8, 3)
                  .IsRequired();

            builder.Property(e => e.Path)
                  .IsRequired()
                  .HasMaxLength(350)
                  .IsUnicode(false);

            builder.Property(e => e.Reference)
                  .HasMaxLength(50)
                  .IsUnicode(false);

            builder.Property(e => e.Comment)
                .HasMaxLength(500)
                .IsUnicode(false);
        }
    }
}
