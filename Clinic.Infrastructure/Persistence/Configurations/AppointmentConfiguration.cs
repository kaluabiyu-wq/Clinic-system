using Clinic.Domain.Entities;
using Clinic.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.HasKey(a => a.Id);

        builder.Ignore(a => a.Reference); // derived from Id

        builder.Property(a => a.Date).IsRequired();
        builder.Property(a => a.SlotTime).IsRequired();
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.IsWalkIn).HasDefaultValue(false);
        builder.Property(a => a.QueueNo).HasMaxLength(10);

        builder.HasOne(a => a.Doctor)
            .WithMany()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // No double booking, enforced by the database: one active, non-walk-in appointment
        // per doctor, date and slot. Cancelled and no-show appointments release the slot.
        // The status names must match the AppointmentStatus enum (stored as text).
        builder.HasIndex(a => new { a.DoctorId, a.Date, a.SlotTime })
            .IsUnique()
            .HasFilter("\"IsWalkIn\" = false AND \"Status\" IN ('Booked','Waiting','Serving','Completed')");

        builder.HasIndex(a => new { a.Date, a.DoctorId });
        builder.HasIndex(a => a.PatientId);
    }
}
