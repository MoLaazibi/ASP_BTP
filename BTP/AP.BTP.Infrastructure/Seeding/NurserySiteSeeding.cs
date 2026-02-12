using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Seeding
{
    public static class NurserySiteSeeding
    {
        public static void Seed(this EntityTypeBuilder<NurserySite> modelBuilder)
        {
            modelBuilder.HasData(
                new NurserySite
                {
                    Id = 1,
                    Name = "Site 1",
                    AddressId = 1,
                    UserId = 2,
                    GroundPlanId = 1,
                }
            );
        }
    }
}
