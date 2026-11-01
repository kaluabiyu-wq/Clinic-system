using Clinic.Application.Abstractions;
using Clinic.Domain.Entities;
using Clinic.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ClinicDbContext _db;

    public AppointmentRepository(ClinicDbContext db) => _db = db;

    public Task<Appointment?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Appointments.AsNoTracking()
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Appointment?> GetByIdForUpdateAsync(int id, CancellationToken ct = default) =>
        _db.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<TimeOnly>> GetTakenSlotsAsync(int doctorId, DateOnly date, CancellationToken ct = default) =>
        await _db.Appointments.AsNoTracking()
            .Where(a => a.DoctorId == doctorId
                     && a.Date == date
                     && !a.IsWalkIn
                     && a.Status != AppointmentStatus.Cancelled
                     && a.Status != AppointmentStatus.NoShow)
            .Select(a => a.SlotTime)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Appointment>> ListAsync(
        DateOnly date, int? doctorId, AppointmentStatus? status, CancellationToken ct = default)
    {
        var query = _db.Appointments.AsNoTracking()
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Where(a => a.Date == date);

        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderBy(a => a.SlotTime)
            .ThenBy(a => a.Id)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken ct = default) =>
        await _db.Appointments.AddAsync(appointment, ct);
}
