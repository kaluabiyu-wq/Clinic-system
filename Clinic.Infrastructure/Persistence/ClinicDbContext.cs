// using Clinic.Application.Abstractions;
using Clinic.Domain.Common;
using Clinic.Domain.Entities;
using Clinic.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence;

public class ClinicDbContext(DbContextOptions<ClinicDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<ScheduleException> ScheduleExceptions => Set<ScheduleException>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentEvent> AppointmentEvents => Set<AppointmentEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); 
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicDbContext).Assembly);
    }

    // public override int SaveChanges(bool acceptAllChangesOnSuccess)
    // {
    //     Stamp();
    //     return base.SaveChanges(acceptAllChangesOnSuccess);
    // }

    // public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
    // {
    //     Stamp();
    //     return base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
    // }

    // private void Stamp()
    // {
    //     var now = clock.UtcNow;
    //     foreach (var entry in ChangeTracker.Entries<BaseEntity>())
    //     {
    //         if (entry.State == EntityState.Added) entry.Entity.CreatedAtUtc = now;
    //         else if (entry.State == EntityState.Modified) entry.Entity.UpdatedAtUtc = now;
    //     }
    // }
}
