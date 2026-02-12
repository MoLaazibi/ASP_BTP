using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Seeding
{
    public static class TaskListSeeding
    {
        public static void Seed(this EntityTypeBuilder<TaskList> modelBuilder)
        {
            modelBuilder.HasData(
                new TaskList()
                {
                    Id = 1,
                    Date = new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc)
                },

                new TaskList()
                {
                    Id = 2,
                    UserId = 2,
                    ZoneId = 1,
                    Date = new DateTime(2025, 11, 11, 0, 0, 0, DateTimeKind.Utc)
                },

                new TaskList()
                {
                    Id = 3,
                    UserId = 2,
                    Date = new DateTime(2025, 11, 12, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}