using Clinic.Domain.Entities;

namespace Clinic.Application.Abstractions;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Matches on name or phone (case-insensitive, partial).</summary>
    Task<IReadOnlyList<Patient>> SearchAsync(string query, int take, CancellationToken ct = default);

    /// <summary>
    /// Same phone number, OR same full name and date of birth.
    /// </summary>
    Task<IReadOnlyList<Patient>> FindPossibleDuplicatesAsync(
        string normalizedPhone, string fullName, DateOnly dateOfBirth, CancellationToken ct = default);

    Task AddAsync(Patient patient, CancellationToken ct = default);
}
