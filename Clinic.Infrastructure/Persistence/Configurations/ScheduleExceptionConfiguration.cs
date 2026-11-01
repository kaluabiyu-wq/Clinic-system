using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class ScheduleExceptionConfiguration : IEntityTypeConfiguration<ScheduleException>
{
    public void Configure(EntityTypeBuilder<ScheduleException> builder)
    {
        builder.ToTable("ScheduleExceptions");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Date).IsRequired();
        builder.Property(c => c.Reason).HasMaxLength(200);

        builder.HasOne<Doctor>()
            .WithMany()
            .HasForeignKey(c => c.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // A doctor can be closed only once per date.
        builder.HasIndex(c => new { c.DoctorId, c.Date }).IsUnique();
    }
}
