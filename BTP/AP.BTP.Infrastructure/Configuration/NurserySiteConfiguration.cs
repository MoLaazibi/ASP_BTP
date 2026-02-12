using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Configuration
{
    public class NurserySiteConfiguration : IEntityTypeConfiguration<NurserySite>
    {
        public void Configure(EntityTypeBuilder<NurserySite> builder)
        {
            builder.ToTable("NurserySite")
                .HasKey(s => s.Id);

            builder.HasIndex(s => s.Id)
                .IsUnique();

            builder.Property(s => s.Id)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(s => s.Name)
                .IsRequired()
                .HasColumnType("varchar(100)");

            builder.Property(s => s.IsArchived)
                .HasDefaultValue(false);

            builder.HasOne(s => s.Address)
                .WithOne(a => a.NurserySite)
                .HasForeignKey<NurserySite>(s => s.AddressId);

            builder.HasOne(s => s.GroundPlan)
                .WithOne(g => g.NurserySite)
                .HasForeignKey<NurserySite>(s => s.GroundPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Zones)
                .WithOne(z => z.NurserySite)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.User)
                .WithOne(u => u.NurserySite)
                .HasForeignKey<NurserySite>(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
