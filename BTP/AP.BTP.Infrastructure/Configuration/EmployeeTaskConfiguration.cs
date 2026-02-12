using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Configuration
{
    public class EmployeeTaskConfiguration : IEntityTypeConfiguration<EmployeeTask>
    {
        public void Configure(EntityTypeBuilder<EmployeeTask> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(512);

            builder.Property(t => t.PlannedDuration)
                .IsRequired();

            builder.Property(t => t.PlannedStartTime)
                .IsRequired();

            builder.HasIndex(t => t.StartTime);

            builder.HasIndex(t => t.StopTime);

            builder.Property(t => t.Order)
                .IsRequired();

            builder.HasIndex(t => t.TaskListId);
            builder.HasIndex(t => new { t.TaskListId, t.Order }).IsUnique(false);

            builder
                .HasOne(t => t.TaskList)
                .WithMany(l => l.Tasks)
                .HasForeignKey(t => t.TaskListId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}