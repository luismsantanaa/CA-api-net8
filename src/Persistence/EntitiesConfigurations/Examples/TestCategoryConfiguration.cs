using Domain.Entities.Examples;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntitiesConfigurations.Examples
{
    public class TestCategoryConfiguration : IEntityTypeConfiguration<TestCategory>
    {
        public void Configure(EntityTypeBuilder<TestCategory> builder)
        {
            builder.ToTable("Categories", "Examples");

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            // Índice para búsquedas por nombre
            builder.HasIndex(c => c.Name);
        }
    }
}

