using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Seeding
{
    public static class UserRoleSeeding
    {
        public static void Seed(this EntityTypeBuilder<UserRole> modelBuilder)
        {
            modelBuilder.HasData(
                new UserRole { Id = 1, UserId = 1, Role = Role.Admin },
                new UserRole { Id = 2, UserId = 2, Role = Role.Medewerker }
            );
        }
    }
}
