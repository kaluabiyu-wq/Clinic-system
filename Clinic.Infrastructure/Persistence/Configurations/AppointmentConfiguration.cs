using Clinic.Domain.Entities;
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

    
        builder.HasIndex(a => new { a.DoctorId, a.Date, a.SlotTime })
            .IsUnique()
            .HasFilter("\"IsWalkIn\" = false AND \"Status\" IN ('Booked','Waiting','Serving','Completed')");

        // Queue numbers are unique per doctor and day. Rows without a number (not checked in yet) are not compared.
        builder.HasIndex(a => new { a.DoctorId, a.Date, a.QueueNo })
            .IsUnique()
            .HasDatabaseName("UX_Appointments_DoctorId_Date_QueueNo");

        // At most one patient in consultation per doctor and day.
        builder.HasIndex(a => new { a.DoctorId, a.Date })
            .IsUnique()
            .HasFilter("\"Status\" = 'Serving'")
            .HasDatabaseName("UX_Appointments_OneServingPerDoctorPerDay");

        builder.HasIndex(a => new { a.Date, a.DoctorId });
        builder.HasIndex(a => a.PatientId);
    }
}
