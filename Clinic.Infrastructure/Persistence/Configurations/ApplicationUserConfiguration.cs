using Clinic.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        b.Property(u => u.FullName).HasMaxLength(120).IsRequired();
        b.Property(u => u.CreatedAtUtc).IsRequired();
        b.HasIndex(u => u.DoctorId);
    }
}
