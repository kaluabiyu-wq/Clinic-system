using Clinic.Application.Abstractions;
using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly ClinicDbContext _db;

    public DoctorRepository(ClinicDbContext db) => _db = db;

    public Task<Doctor?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<Doctor?> GetByIdForUpdateAsync(int id, CancellationToken ct = default) =>
        _db.Doctors.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<Doctor>> GetAllAsync(bool includeInactive, CancellationToken ct = default)
    {
        var query = _db.Doctors.AsNoTracking();

        if (!includeInactive)
            query = query.Where(d => d.IsActive);

        return await query.OrderBy(d => d.FullName).ToListAsync(ct);
    }

    public Task<bool> QueueLetterExistsAsync(string queueLetter, CancellationToken ct = default) =>
        _db.Doctors.AnyAsync(d => d.QueueLetter == queueLetter, ct);

    public async Task AddAsync(Doctor doctor, CancellationToken ct = default) =>
        await _db.Doctors.AddAsync(doctor, ct);
}
