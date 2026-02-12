using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Seeding
{
    public static class UserSeeding
    {
        public static void Seed(this EntityTypeBuilder<User> modelBuilder)
        {
            modelBuilder.HasData(
                new User
                {
                    Id = 1,
                    AuthId = "6910ac6f6749b5a63269405d",
                    Email = "testadmin@test.com",
                    Username = "TestAdmin",
                    FirstName = "FirstAdmin",
                    LastName = "LastAdmin"
                },
                new User
                {
                    Id = 2,
                    AuthId = "6910b5346749b5a6326942b8",
                    Email = "testmedewerker@test.com",
                    Username = "TestMedewerker",
                    FirstName = "FirstMedewerker",
                    LastName = "LastMedewerker"
                }
            );
        }
    }
}

