using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("Schedules", t =>
        {
            t.HasCheckConstraint("CK_Schedules_EndAfterStart", "\"EndTime\" > \"StartTime\"");
            t.HasCheckConstraint("CK_Schedules_SlotMinutes", "\"SlotMinutes\" > 0");
        });
        builder.HasKey(s => s.Id);

        builder.Property(s => s.DayOfWeek).HasConversion<string>().HasMaxLength(10);
        builder.Property(s => s.StartTime).HasColumnType("time without time zone");
        builder.Property(s => s.EndTime).HasColumnType("time without time zone");
        builder.Property(s => s.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(s => s.UpdatedAt).HasColumnType("timestamp with time zone");

        // One working block per doctor per weekday keeps SlotGenerator unambiguous.
        builder.HasIndex(s => new { s.DoctorId, s.DayOfWeek }).IsUnique();
    }
}
