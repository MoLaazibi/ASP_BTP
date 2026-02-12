using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Configuration
{
    public class TaskListConfiguration : IEntityTypeConfiguration<TaskList>
    {
        public void Configure(EntityTypeBuilder<TaskList> builder)
        {
            builder
                .HasKey(tl => tl.Id);

            builder.Property(tl => tl.Date)
                .IsRequired();

            builder.Property(tl => tl.IsArchived)
                .HasDefaultValue(false);

            builder
                .HasMany(tl => tl.Tasks)
                .WithOne(t => t.TaskList)
                .HasForeignKey(t => t.TaskListId)
                .OnDelete(DeleteBehavior.SetNull);

            builder
                .HasIndex(tl => tl.Date);

            builder.HasOne(tl => tl.User)
                .WithMany(u => u.TaskLists)
                .HasForeignKey(tl => tl.UserId);

            builder.HasOne(tl => tl.Zone)
                .WithMany(z => z.TaskLists)
                .HasForeignKey(tl => tl.ZoneId);
        }
    }
}
