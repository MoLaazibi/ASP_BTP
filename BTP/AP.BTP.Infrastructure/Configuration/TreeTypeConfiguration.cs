using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Configuration
{
    public class TreeTypeConfiguration : IEntityTypeConfiguration<TreeType>
    {
        public void Configure(EntityTypeBuilder<TreeType> builder)
        {
            builder.ToTable("TreeType").HasKey(t => t.Id);

            builder
                .HasIndex(t => t.Id).IsUnique();

            builder.Property(t => t.Id)
                .IsRequired()
                .HasColumnType("int");


            builder.Property(t => t.IsArchived)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("IsArchived");




            builder.Property(t => t.Name)
                .IsRequired()
                .HasColumnType("varchar(100)");


            builder.Property(t => t.Description)
                .IsRequired()
                .HasColumnType("varchar(255)");


            builder.HasMany(t => t.TreeImages)
                   .WithOne()
                   .HasForeignKey("TreeTypeId")
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
