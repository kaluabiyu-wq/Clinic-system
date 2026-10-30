using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class AppointmentEventConfiguration : IEntityTypeConfiguration<AppointmentEvent>
{
    public void Configure(EntityTypeBuilder<AppointmentEvent> builder)
    {
        builder.ToTable("AppointmentEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FromStatus).HasConversion<string>().HasMaxLength(16);
        builder.Property(e => e.ToStatus).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(e => e.OccurredAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(e => e.UserId).HasMaxLength(450); // matches Identity's key length
        builder.Property(e => e.Note).HasMaxLength(300);

        // History of one appointment, read in time order.
        builder.HasIndex(e => new { e.AppointmentId, e.OccurredAtUtc });
        builder.HasIndex(e => e.UserId);
    }
}
