using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FullName).IsRequired().HasMaxLength(120);
        builder.Property(p => p.Phone).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Address).HasMaxLength(200);
        builder.Property(p => p.Sex).HasConversion<string>().HasMaxLength(10);
        builder.Property(p => p.DateOfBirth).HasColumnType("date");
        builder.Property(p => p.CreatedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(p => p.UpdatedAtUtc).HasColumnType("timestamp with time zone");

        // Duplicate warning (FR2) looks up by phone, then by name + birth date.
        // Deliberately NOT unique: two relatives can share a phone.
        builder.HasIndex(p => p.Phone);
        builder.HasIndex(p => new { p.FullName, p.DateOfBirth });

        builder.HasMany(p => p.Appointments)
               .WithOne(a => a.Patient)
               .HasForeignKey(a => a.PatientId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
