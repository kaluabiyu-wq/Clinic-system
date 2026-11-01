using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class AppointmentEventConfiguration : IEntityTypeConfiguration<AppointmentEvent>
{
    public void Configure(EntityTypeBuilder<AppointmentEvent> builder)
    {
        builder.ToTable("AppointmentEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.ToStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.Note).HasMaxLength(500);
        builder.Property(e => e.ChangedByUserId).HasMaxLength(450); // same length as the Identity key

        builder.HasOne<Appointment>()
            .WithMany(a => a.Events)
            .HasForeignKey(e => e.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.AppointmentId);
    }
}
