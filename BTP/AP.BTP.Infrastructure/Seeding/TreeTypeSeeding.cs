using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Seeding
{
    public static class TreeTypeSeeding
    {
        public static void Seed(this EntityTypeBuilder<TreeType> modelBuilder)
        {
            modelBuilder.HasData(
                new TreeType
                {
                    Id = 1,
                    Name = "Haagbeuk",
                    Description = "Testbeschrijving",
                    IsArchived = false
                }
            );
        }
    }
}
