using Clinic.Domain.Entities;
using Clinic.Domain.Enums;

namespace Clinic.Application.Abstractions;

public interface IAppointmentRepository
{
    /// <summary>Read-only, with Doctor and Patient loaded.</summary>
    Task<Appointment?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Tracked, with Doctor and Patient loaded, so changes are saved by IUnitOfWork.</summary>
    Task<Appointment?> GetByIdForUpdateAsync(int id, CancellationToken ct = default);

    /// <summary>Start times held by active (not cancelled, not no-show), non-walk-in appointments.</summary>
    Task<IReadOnlyList<TimeOnly>> GetTakenSlotsAsync(int doctorId, DateOnly date, CancellationToken ct = default);

    Task<IReadOnlyList<Appointment>> ListAsync(
        DateOnly date, int? doctorId, AppointmentStatus? status, CancellationToken ct = default);

    Task AddAsync(Appointment appointment, CancellationToken ct = default);
}
