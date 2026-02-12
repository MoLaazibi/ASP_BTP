using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Configuration
{
    public class GroundPlanConfiguration : IEntityTypeConfiguration<GroundPlan>
    {
        public void Configure(EntityTypeBuilder<GroundPlan> builder)
        {
            builder.ToTable("GroundPlan")
                .HasKey(g => g.Id);

            builder.HasIndex(g => g.Id)
                .IsUnique();

            builder.Property(g => g.Id)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(g => g.FileUrl)
                .IsRequired()
                .HasColumnType("varchar(255)");

            builder.Property(g => g.UploadTime)
                .IsRequired()
                .HasColumnType("datetime2(7)");

            builder.HasOne(g => g.NurserySite)
                .WithOne(n => n.GroundPlan);
        }
    }
}
