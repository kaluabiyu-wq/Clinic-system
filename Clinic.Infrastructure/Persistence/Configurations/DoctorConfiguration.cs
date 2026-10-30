using Clinic.Domain.Entities;
using Clinic.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors", t =>
        {
            t.HasCheckConstraint("CK_Doctors_AvgConsultMinutes", "\"AvgConsultMinutes\" > 0");
            t.HasCheckConstraint("CK_Doctors_QueueLetter", "char_length(\"QueueLetter\") = 1");
        });
        builder.HasKey(d => d.Id);

        builder.Property(d => d.FullName).IsRequired().HasMaxLength(120);
        builder.Property(d => d.Specialty).IsRequired().HasMaxLength(80);
        builder.Property(d => d.QueueLetter).IsRequired().HasMaxLength(1);
        builder.Property(d => d.RoomName).HasMaxLength(40);
        builder.Property(d => d.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(d => d.UpdatedAt).HasColumnType("timestamp with time zone");

        // Two doctors sharing a letter would produce identical queue labels on the board.
        builder.HasIndex(d => d.QueueLetter).IsUnique();

        // Link to Identity without giving the Domain a reference to ApplicationUser:
        // the generic HasOne<T>() form needs no navigation property on either class.
        builder.HasOne<ApplicationUser>()
               .WithOne()
               .HasForeignKey<Doctor>(d => d.UserId)
               .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(d => d.UserId).IsUnique().HasFilter("\"UserId\" IS NOT NULL");

        builder.HasMany(d => d.Schedules).WithOne(s => s.Doctor)
               .HasForeignKey(s => s.DoctorId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(d => d.Exceptions).WithOne(e => e.Doctor)
               .HasForeignKey(e => e.DoctorId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(d => d.Appointments).WithOne(a => a.Doctor)
               .HasForeignKey(a => a.DoctorId).OnDelete(DeleteBehavior.Restrict);
    }
}
