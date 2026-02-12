using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Configuration
{
    internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Category")
                .HasKey(c => c.Id);

            builder.HasIndex(c => c.Id)
                .IsUnique();

            builder.Property(c => c.Id)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(c => c.Color)
                .IsRequired()
                .HasColumnType("varchar(20)");

            builder.Property(c => c.Name)
                .IsRequired()
                .HasColumnType("varchar(30)");

            builder.Property(c => c.IsArchived)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnType("bit");
        }

    }
}
