using Clinic.Domain.Entities;

namespace Clinic.Application.Abstractions;

public interface IDoctorRepository
{
    /// <summary>Read-only (not tracked).</summary>
    Task<Doctor?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Tracked, so changes are saved by IUnitOfWork.</summary>
    Task<Doctor?> GetByIdForUpdateAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Doctor>> GetAllAsync(bool includeInactive, CancellationToken ct = default);

    Task<bool> QueueLetterExistsAsync(string queueLetter, CancellationToken ct = default);

    Task AddAsync(Doctor doctor, CancellationToken ct = default);
}
