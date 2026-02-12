using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Seeding
{
    public static class GroundPlanSeeding
    {
        public static void Seed(this EntityTypeBuilder<GroundPlan> modelBuilder)
        {
            modelBuilder.HasData(
                new GroundPlan
                {
                    Id = 1,
                    FileUrl = "/test/file",
                    UploadTime = DateTime.Now.Date,
                }
            );
        }
    }
}
