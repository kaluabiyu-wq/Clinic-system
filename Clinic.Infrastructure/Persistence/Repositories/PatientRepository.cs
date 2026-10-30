using Clinic.Application.Abstractions;
using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly ClinicDbContext _db;

    public PatientRepository(ClinicDbContext db) => _db = db;

    public Task<Patient?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Patient>> SearchAsync(string query, int take, CancellationToken ct = default)
    {
        var pattern = $"%{EscapeLike(query)}%";

        return await _db.Patients
            .AsNoTracking()
            .Where(p => EF.Functions.ILike(p.FullName, pattern, "\\")
                     || EF.Functions.ILike(p.PhoneNumber, pattern, "\\"))
            .OrderBy(p => p.FullName)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Patient>> FindPossibleDuplicatesAsync(
        string normalizedPhone, string fullName, DateOnly dateOfBirth, CancellationToken ct = default)
    {
        var nameLower = fullName.ToLower();

        return await _db.Patients
            .AsNoTracking()
            .Where(p => p.PhoneNumber == normalizedPhone
                     || (p.DateOfBirth == dateOfBirth && p.FullName.ToLower() == nameLower))
            .OrderBy(p => p.FullName)
            .Take(10)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Patient patient, CancellationToken ct = default) =>
        await _db.Patients.AddAsync(patient, ct);

    private static string EscapeLike(string input) =>
        input.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
