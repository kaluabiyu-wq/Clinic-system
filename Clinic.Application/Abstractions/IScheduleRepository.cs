using Clinic.Domain.Entities;

namespace Clinic.Application.Abstractions;

public interface IScheduleRepository
{
    // Weekly schedule
    Task<IReadOnlyList<Schedule>> GetByDoctorAsync(int doctorId, CancellationToken ct = default);
    Task<Schedule?> GetByDoctorAndDayAsync(int doctorId, DayOfWeek dayOfWeek, CancellationToken ct = default);

    /// <summary>Tracked, so changes are saved by IUnitOfWork.</summary>
    Task<IReadOnlyList<Schedule>> GetByDoctorForUpdateAsync(int doctorId, CancellationToken ct = default);

    Task AddAsync(Schedule schedule, CancellationToken ct = default);
    void Remove(Schedule schedule);

    // Closures (ScheduleException)
    Task<IReadOnlyList<ScheduleException>> GetClosuresFromAsync(int doctorId, DateOnly from, CancellationToken ct = default);
    Task<ScheduleException?> GetClosureAsync(int doctorId, int closureId, CancellationToken ct = default);
    Task<bool> ClosureExistsAsync(int doctorId, DateOnly date, CancellationToken ct = default);
    Task AddClosureAsync(ScheduleException closure, CancellationToken ct = default);
    void RemoveClosure(ScheduleException closure);
}
