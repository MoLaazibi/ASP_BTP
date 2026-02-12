using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.AuthId)
                .IsRequired();

            builder.Property(u => u.Email)
                .IsRequired();

            builder.Property(u => u.Username)
                .IsRequired();

            builder.Property(u => u.FirstName)
                .HasColumnType("varchar(100)");

            builder.Property(u => u.LastName)
                .HasColumnType("varchar(100)");

            builder.HasMany(u => u.Roles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.TaskLists)
                .WithOne(l => l.User);

            builder.HasOne(u => u.NurserySite)
                .WithOne(l => l.User);

            builder.HasOne(u => u.PreferredNurserySite)
                .WithMany()
                .HasForeignKey(u => u.PreferredNurserySiteId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
