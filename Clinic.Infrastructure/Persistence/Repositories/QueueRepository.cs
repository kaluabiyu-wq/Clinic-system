using Clinic.Application.Abstractions;
using Clinic.Domain.Entities;
using Clinic.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence.Repositories;

public class QueueRepository : IQueueRepository
{
    private readonly ClinicDbContext _db;

    public QueueRepository(ClinicDbContext db) => _db = db;

    public Task<int> CountQueueNumbersIssuedAsync(int doctorId, DateOnly date, CancellationToken ct = default) =>
        _db.Appointments.CountAsync(a => a.DoctorId == doctorId && a.Date == date && a.QueueNo != null, ct);

    public async Task<IReadOnlyList<Appointment>> GetWaitingAsync(
        int doctorId, DateOnly date, bool includePatient = false, CancellationToken ct = default)
    {
        var query = _db.Appointments.AsNoTracking().AsQueryable();

        if (includePatient)
            query = query.Include(a => a.Patient);

        return await query
            .Where(a => a.DoctorId == doctorId && a.Date == date && a.Status == AppointmentStatus.Waiting)
            .OrderBy(a => a.CheckInAt)
            .ThenBy(a => a.Id)
            .ToListAsync(ct);
    }

    public Task<Appointment?> GetServingAsync(
        int doctorId, DateOnly date, bool includePatient = false, CancellationToken ct = default)
    {
        var query = _db.Appointments.AsNoTracking().AsQueryable();

        if (includePatient)
            query = query.Include(a => a.Patient);

        return query.FirstOrDefaultAsync(
            a => a.DoctorId == doctorId && a.Date == date && a.Status == AppointmentStatus.Serving, ct);
    }

    public Task<Appointment?> GetNextWaitingForUpdateAsync(int doctorId, DateOnly date, CancellationToken ct = default) =>
        _db.Appointments
            .Where(a => a.DoctorId == doctorId && a.Date == date && a.Status == AppointmentStatus.Waiting)
            .OrderBy(a => a.CheckInAt)
            .ThenBy(a => a.Id)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Appointment>> GetActiveQueueAsync(DateOnly date, CancellationToken ct = default) =>
        await _db.Appointments.AsNoTracking()
            .Where(a => a.Date == date
                     && (a.Status == AppointmentStatus.Waiting || a.Status == AppointmentStatus.Serving))
            .OrderBy(a => a.CheckInAt)
            .ThenBy(a => a.Id)
            .ToListAsync(ct);
}
