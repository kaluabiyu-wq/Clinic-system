using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class ScheduleExceptionConfiguration : IEntityTypeConfiguration<ScheduleException>
{
    public void Configure(EntityTypeBuilder<ScheduleException> builder)
    {
        builder.ToTable("ScheduleExceptions", t =>
        {
            // Either a whole-day closure (both null) or a proper window (both set, ordered).
            t.HasCheckConstraint("CK_ScheduleExceptions_Window",
                "(\"StartTime\" IS NULL AND \"EndTime\" IS NULL) OR " +
                "(\"StartTime\" IS NOT NULL AND \"EndTime\" IS NOT NULL AND \"EndTime\" > \"StartTime\")");
        });
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Date).HasColumnType("date");
        builder.Property(e => e.StartTime).HasColumnType("time without time zone");
        builder.Property(e => e.EndTime).HasColumnType("time without time zone");
        builder.Property(e => e.Reason).HasMaxLength(200);
        builder.Property(e => e.CreatedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(e => e.UpdatedAtUtc).HasColumnType("timestamp with time zone");

        // The slot query asks "closures for this doctor on this date" on every request.
        builder.HasIndex(e => new { e.DoctorId, e.Date });
    }
}
