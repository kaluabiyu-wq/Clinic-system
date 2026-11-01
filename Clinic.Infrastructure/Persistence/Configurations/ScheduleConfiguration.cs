using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("Schedules", t =>
        {
            t.HasCheckConstraint("CK_Schedules_CloseAfterOpen", "\"CloseTime\" > \"OpenTime\"");
            t.HasCheckConstraint("CK_Schedules_SlotMinutesPositive", "\"SlotMinutes\" > 0");
        });

        builder.HasKey(s => s.Id);

        builder.Property(s => s.DayOfWeek).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(s => s.OpenTime).IsRequired();
        builder.Property(s => s.CloseTime).IsRequired();
        builder.Property(s => s.SlotMinutes).IsRequired();

        builder.HasOne<Doctor>()
            .WithMany()
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // One row per doctor per weekday.
        builder.HasIndex(s => new { s.DoctorId, s.DayOfWeek }).IsUnique();
    }
}
