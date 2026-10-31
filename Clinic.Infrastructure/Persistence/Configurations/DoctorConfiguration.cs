using Clinic.Domain.Constants;
using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.FullName).IsRequired().HasMaxLength(150);
        builder.Property(d => d.Specialty).IsRequired().HasMaxLength(100);
        builder.Property(d => d.QueueLetter).IsRequired().HasMaxLength(1).IsFixedLength();
        builder.Property(d => d.RoomNumber).HasMaxLength(20);
        builder.Property(d => d.AvgConsultMinutes).HasDefaultValue(ClinicDefaults.AvgConsultMinutes);
        builder.Property(d => d.IsActive).HasDefaultValue(true);
        builder.Property(d => d.UserId).HasMaxLength(450); // same length as the Identity key

        // Queue numbers like G-005 must identify exactly one doctor.
        builder.HasIndex(d => d.QueueLetter).IsUnique();
        builder.HasIndex(d => d.IsActive);
    }
}
