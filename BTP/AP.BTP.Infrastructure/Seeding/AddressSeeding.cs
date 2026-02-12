using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Seeding
{
    public static class AddressSeeding
    {
        public static void Seed(this EntityTypeBuilder<Address> modelBuilder)
        {
            modelBuilder.HasData(
                new Address
                {
                    Id = 1,
                    StreetName = "teststraat",
                    HouseNumber = "A22",
                    PostalCode = "2140"
                }
            );
        }
    }
}
