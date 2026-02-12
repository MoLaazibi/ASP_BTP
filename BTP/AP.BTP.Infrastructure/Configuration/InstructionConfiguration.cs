using AP.BTP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AP.BTP.Infrastructure.Configuration
{
    public class InstructionConfiguration : IEntityTypeConfiguration<Instruction>
    {
        public void Configure(EntityTypeBuilder<Instruction> builder)
        {
            builder.ToTable("Instruction");

            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id)
                .HasColumnName("instructieId")
                .IsRequired()
                .HasColumnType("int");

            builder.Property(i => i.TreeTypeId)
                .HasColumnName("boomSoortId")
                .IsRequired()
                .HasColumnType("int");

            builder.Property(i => i.Season)
                .HasColumnName("seizoen")
                .IsRequired()
                .HasColumnType("varchar(20)");

            builder.Property(i => i.FileUrl)
                .HasColumnName("bestandUrl")
                .IsRequired()
                .HasColumnType("varchar(255)");

            builder.Property(i => i.UploadTime)
                .HasColumnName("uploadTijd")
                .IsRequired()
                .HasColumnType("date");

            builder.HasOne(i => i.TreeType)
                .WithMany(t => t.Instructions)
                .HasForeignKey(i => i.TreeTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
