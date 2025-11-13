using Domain.Entities.Examples;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntitiesConfigurations.Examples
{
    public class TestProductConfiguration : IEntityTypeConfiguration<TestProduct>
    {
        public void Configure(EntityTypeBuilder<TestProduct> builder)
        {
            builder.ToTable("Products", "Examples");

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Stock)
                .IsRequired();

            // Relación unidireccional con Category
            builder.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(p => p.Name);
            builder.HasIndex(p => p.CategoryId);
        }
    }
}

