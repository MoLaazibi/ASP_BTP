using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Configuration
{
    internal class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("Address")
                .HasKey(a => a.Id);

            builder.HasIndex(a => a.Id)
                .IsUnique();

            builder.Property(a => a.Id)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(a => a.StreetName)
                .IsRequired()
                .HasColumnType("varchar(100)");

            builder.Property(a => a.PostalCode)
                .IsRequired()
                .HasColumnType("varchar(10)");

            builder.Property(a => a.HouseNumber)
                .IsRequired()
                .HasColumnType("varchar(10)");
        }
    }
}
