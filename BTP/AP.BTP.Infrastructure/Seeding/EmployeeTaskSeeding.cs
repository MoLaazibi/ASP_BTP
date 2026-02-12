using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Seeding
{
    public static class EmployeeTaskSeeding
    {
        public static void Seed(this EntityTypeBuilder<EmployeeTask> modelBuilder)
        {
            modelBuilder.HasData(
                new EmployeeTask()
                {
                    Id = 1,
                    Description = "Dit is taak 1",
                    PlannedDuration = 2,
                    PlannedStartTime = new DateTime(2025, 10, 1, 9, 0, 0, DateTimeKind.Utc),
                    StartTime = new DateTime(2025, 10, 1, 9, 0, 0, DateTimeKind.Utc),
                    StopTime = new DateTime(2025, 10, 1, 11, 30, 0, DateTimeKind.Utc),
                    Order = 1,
                    TaskListId = 1,
                    CategoryId = 1
                },

                new EmployeeTask()
                {
                    Id = 2,
                    Description = "Dit is taak 2",
                    PlannedDuration = 1,
                    PlannedStartTime = new DateTime(2025, 10, 1, 13, 0, 0, DateTimeKind.Utc),
                    StartTime = new DateTime(2025, 10, 1, 13, 0, 0, DateTimeKind.Utc),
                    StopTime = new DateTime(2025, 10, 1, 14, 0, 0, DateTimeKind.Utc),
                    Order = 2,
                    TaskListId = 1,
                    CategoryId = 1
                },

                new EmployeeTask()
                {
                    Id = 3,
                    Description = "Dit is taak 3",
                    PlannedDuration = 3,
                    PlannedStartTime = new DateTime(2025, 10, 1, 14, 0, 0, DateTimeKind.Utc),
                    StartTime = new DateTime(2025, 10, 1, 14, 0, 0, DateTimeKind.Utc),
                    StopTime = new DateTime(2025, 10, 1, 18, 0, 0, DateTimeKind.Utc),
                    Order = 3,
                    TaskListId = 1,
                    CategoryId = 1
                },

                new EmployeeTask()
                {
                    Id = 4,
                    Description = "Dit is taak 4",
                    PlannedDuration = 1,
                    PlannedStartTime = new DateTime(2025, 11, 11, 9, 0, 0, DateTimeKind.Utc),
                    StartTime = new DateTime(2025, 11, 11, 9, 0, 0, DateTimeKind.Utc),
                    StopTime = new DateTime(2025, 11, 11, 10, 30, 0, DateTimeKind.Utc),
                    Order = 1,
                    TaskListId = 2,
                    CategoryId = 1
                },

                new EmployeeTask()
                {
                    Id = 5,
                    Description = "Dit is taak 5",
                    PlannedDuration = 1,
                    PlannedStartTime = new DateTime(2025, 11, 11, 11, 0, 0, DateTimeKind.Utc),
                    StartTime = new DateTime(2025, 11, 11, 11, 0, 0, DateTimeKind.Utc),
                    StopTime = new DateTime(2025, 11, 11, 12, 30, 0, DateTimeKind.Utc),
                    Order = 2,
                    TaskListId = 2,
                    CategoryId = 1
                },

                new EmployeeTask()
                {
                    Id = 6,
                    Description = "Dit is taak 6",
                    PlannedDuration = 1,
                    PlannedStartTime = new DateTime(2025, 11, 11, 13, 0, 0, DateTimeKind.Utc),
                    StartTime = new DateTime(2025, 11, 11, 13, 0, 0, DateTimeKind.Utc),
                    StopTime = new DateTime(2025, 11, 11, 14, 0, 0, DateTimeKind.Utc),
                    Order = 3,
                    TaskListId = 2,
                    CategoryId = 1
                },

                new EmployeeTask()
                {
                    Id = 7,
                    Description = "Dit is taak 7",
                    PlannedDuration = 1,
                    PlannedStartTime = new DateTime(2025, 11, 12, 9, 0, 0, DateTimeKind.Utc),
                    StartTime = new DateTime(2025, 11, 12, 9, 0, 0, DateTimeKind.Utc),
                    StopTime = new DateTime(2025, 11, 12, 9, 30, 0, DateTimeKind.Utc),
                    Order = 4,
                    TaskListId = 3,
                    CategoryId = 1
                }
            );
        }
    }
}