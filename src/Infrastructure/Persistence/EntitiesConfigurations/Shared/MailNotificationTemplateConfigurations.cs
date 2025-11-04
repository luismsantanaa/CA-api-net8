using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Constants;

namespace Persistence.EntitiesConfigurations.Shared
{
    internal class MailNotificationTemplateConfigurations : IEntityTypeConfiguration<MailNotificationTemplate>
    {
        public void Configure(EntityTypeBuilder<MailNotificationTemplate> builder)
        {
            builder.ToTable("MailNotificationsTemplates", SchemasDb.Shared);

            builder.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(200);

            builder.Property(e => e.Suject)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(e => e.BodyHtml)
                  .HasMaxLength(4000);

            builder.Property(e => e.PathImages)
                   .HasMaxLength(500);
        }
    }
}
