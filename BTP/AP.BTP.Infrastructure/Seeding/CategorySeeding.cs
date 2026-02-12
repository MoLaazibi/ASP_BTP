using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Seeding
{
    public static class CategorySeeding
    {
        public static void Seed(this EntityTypeBuilder<Category> modelBuilder)
        {
            modelBuilder.HasData(
                new Category
                {
                    Id = 1,
                    Name = "Watergeven",
                    Color = "#111999",
                    IsArchived = false,
                }
            );
        }
    }
}
