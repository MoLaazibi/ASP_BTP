using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Configuration
{
    internal class ZoneConfiguration : IEntityTypeConfiguration<Zone>
    {
        public void Configure(EntityTypeBuilder<Zone> builder)
        {
            builder.ToTable("Zone")
                .HasKey(z => z.Id);

            builder.HasIndex(z => z.Id)
                .IsUnique();

            builder.Property(z => z.Id)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(z => z.Code)
                .IsRequired()
                .HasColumnType("varchar(10)");

            builder.Property(z => z.Size)
                .IsRequired()
                .HasColumnType("decimal(6,2)");

            builder.HasOne(z => z.NurserySite)
                .WithMany(s => s.Zones)
                .HasForeignKey(z => z.NurserySiteId)
                .IsRequired();

            builder.HasOne(z => z.TreeType)
                .WithMany(t => t.Zones)
                .HasForeignKey(z => z.TreeTypeId)
                .IsRequired();

            builder.HasMany(z => z.TaskLists)
                .WithOne(tl => tl.Zone);
        }
    }
}
