using Clinic.Application.Abstractions;
using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence.Repositories;

public class ScheduleRepository : IScheduleRepository
{
    private readonly ClinicDbContext _db;

    public ScheduleRepository(ClinicDbContext db) => _db = db;


    public async Task<IReadOnlyList<Schedule>> GetByDoctorAsync(int doctorId, CancellationToken ct = default) =>
        await _db.Schedules.AsNoTracking()
            .Where(s => s.DoctorId == doctorId)
            .ToListAsync(ct);

    public Task<Schedule?> GetByDoctorAndDayAsync(int doctorId, DayOfWeek dayOfWeek, CancellationToken ct = default) =>
        _db.Schedules.AsNoTracking()
            .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == dayOfWeek, ct);

    public async Task<IReadOnlyList<Schedule>> GetByDoctorForUpdateAsync(int doctorId, CancellationToken ct = default) =>
        await _db.Schedules
            .Where(s => s.DoctorId == doctorId)
            .ToListAsync(ct);

    public async Task AddAsync(Schedule schedule, CancellationToken ct = default) =>
        await _db.Schedules.AddAsync(schedule, ct);

    public void Remove(Schedule schedule) => _db.Schedules.Remove(schedule);



    public async Task<IReadOnlyList<ScheduleException>> GetClosuresFromAsync(int doctorId, DateOnly from, CancellationToken ct = default) =>
        await _db.ScheduleExceptions.AsNoTracking()
            .Where(c => c.DoctorId == doctorId && c.Date >= from)
            .OrderBy(c => c.Date)
            .ToListAsync(ct);

    public Task<ScheduleException?> GetClosureAsync(int doctorId, int closureId, CancellationToken ct = default) =>
        _db.ScheduleExceptions
            .FirstOrDefaultAsync(c => c.Id == closureId && c.DoctorId == doctorId, ct);

    public Task<bool> ClosureExistsAsync(int doctorId, DateOnly date, CancellationToken ct = default) =>
        _db.ScheduleExceptions.AnyAsync(c => c.DoctorId == doctorId && c.Date == date, ct);

    public async Task AddClosureAsync(ScheduleException closure, CancellationToken ct = default) =>
        await _db.ScheduleExceptions.AddAsync(closure, ct);

    public void RemoveClosure(ScheduleException closure) => _db.ScheduleExceptions.Remove(closure);
}
