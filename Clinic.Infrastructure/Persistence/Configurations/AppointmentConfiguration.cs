using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments", t =>
        {
            // A walk-in has no slot; a booked appointment must have one.
            t.HasCheckConstraint("CK_Appointments_SlotMatchesKind",
                "(\"IsWalkIn\" = TRUE AND \"SlotTime\" IS NULL) OR " +
                "(\"IsWalkIn\" = FALSE AND \"SlotTime\" IS NOT NULL)");
            t.HasCheckConstraint("CK_Appointments_QueueNoPositive",
                "\"QueueNo\" IS NULL OR \"QueueNo\" > 0");
        });
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Reference).IsRequired().HasMaxLength(12);
        builder.Property(a => a.Date).HasColumnType("date");
        builder.Property(a => a.SlotTime).HasColumnType("time without time zone");
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(a => a.Reason).HasMaxLength(300);
        builder.Property(a => a.CancelReason).HasMaxLength(300);

        builder.Property(a => a.CheckInAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(a => a.StartAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(a => a.EndAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(a => a.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(a => a.UpdatedAt).HasColumnType("timestamp with time zone");

        // Optimistic concurrency on PostgreSQL's xmin system column (uint).
        builder.Property(a => a.Version).IsRowVersion();

        // Derived value, not a column.
        builder.Ignore(a => a.WaitTime);

        // 1. Public lookup by reference (track queue, cancel, reschedule).
        builder.HasIndex(a => a.Reference).IsUnique();

        // 2. No double booking: enforced by the database, not only by code.
        //    Cancelled and NoShow are excluded so those slots become bookable again;
        //    walk-ins are excluded because they hold no slot.
        builder.HasIndex(a => new { a.DoctorId, a.Date, a.SlotTime })
               .IsUnique()
               .HasDatabaseName("UX_Appointments_ActiveSlot")
               .HasFilter("\"IsWalkIn\" = false AND \"Status\" IN ('Booked','Waiting','Serving','Completed')");

        // 3. Unique queue numbers per doctor per day, checked at check-in.
        builder.HasIndex(a => new { a.DoctorId, a.Date, a.QueueNo })
               .IsUnique()
               .HasDatabaseName("UX_Appointments_QueueNo")
               .HasFilter("\"QueueNo\" IS NOT NULL");

        // 4. Reception list and queue board: one doctor, one day, filtered by status.
        builder.HasIndex(a => new { a.Date, a.DoctorId, a.Status });

        builder.HasIndex(a => a.PatientId);

        builder.HasMany(a => a.Events)
               .WithOne(e => e.Appointment)
               .HasForeignKey(e => e.AppointmentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
