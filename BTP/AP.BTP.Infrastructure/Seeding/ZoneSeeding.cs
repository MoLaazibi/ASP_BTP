using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Seeding
{
    public static class ZoneSeeding
    {
        public static void Seed(this EntityTypeBuilder<Zone> modelBuilder)
        {
            modelBuilder.HasData(
                new Zone
                {
                    Id = 1,
                    Code = "A1",
                    Size = 20,
                    NurserySiteId = 1,
                    TreeTypeId = 1
                }
            );
        }
    }
}
