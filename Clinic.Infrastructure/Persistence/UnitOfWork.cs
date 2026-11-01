using System.Data.Common;
using Clinic.Application.Abstractions;
using Clinic.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    // PostgreSQL error code for "unique_violation".
    private const string UniqueViolation = "23505";

    private readonly ClinicDbContext _db;

    public UnitOfWork(ClinicDbContext db) => _db = db;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is DbException { SqlState: UniqueViolation })
        {
            throw new ConflictException("The data conflicts with an existing record.");
        }
    }
}
