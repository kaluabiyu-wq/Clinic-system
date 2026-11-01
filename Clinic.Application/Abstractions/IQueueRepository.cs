using Clinic.Domain.Entities;

namespace Clinic.Application.Abstractions;

public interface IQueueRepository
{
    /// <summary>How many queue numbers this doctor has already issued on that date (including cancelled ones).</summary>
    Task<int> CountQueueNumbersIssuedAsync(int doctorId, DateOnly date, CancellationToken ct = default);

    /// <summary>Waiting patients in arrival order (check-in time, then id).</summary>
    Task<IReadOnlyList<Appointment>> GetWaitingAsync(
        int doctorId, DateOnly date, bool includePatient = false, CancellationToken ct = default);

    /// <summary>The patient currently in consultation, if any.</summary>
    Task<Appointment?> GetServingAsync(
        int doctorId, DateOnly date, bool includePatient = false, CancellationToken ct = default);

    /// <summary>Tracked: the first waiting patient in arrival order, ready to be called.</summary>
    Task<Appointment?> GetNextWaitingForUpdateAsync(int doctorId, DateOnly date, CancellationToken ct = default);

    /// <summary>Everyone Waiting or Serving on that date, for all doctors. Used by the public board.</summary>
    Task<IReadOnlyList<Appointment>> GetActiveQueueAsync(DateOnly date, CancellationToken ct = default);
}
