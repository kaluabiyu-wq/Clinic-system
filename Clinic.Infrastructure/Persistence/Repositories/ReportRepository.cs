using Clinic.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly ClinicDbContext _db;

    public ReportRepository(ClinicDbContext db) => _db = db;

    public async Task<IReadOnlyList<ReportRow>> GetRowsAsync(
        DateOnly from, DateOnly to, int? doctorId, CancellationToken ct = default)
    {
        var query = _db.Appointments.AsNoTracking()
            .Where(a => a.Date >= from && a.Date <= to);

        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId.Value);

        // Only the columns reports need. Patient is never joined, so no patient data is read.
        return await query
            .OrderBy(a => a.Date)
            .ThenBy(a => a.SlotTime)
            .ThenBy(a => a.Id)
            .Select(a => new ReportRow(
                a.Id,
                a.DoctorId,
                a.Doctor!.FullName,
                a.Date,
                a.SlotTime,
                a.Status,
                a.IsWalkIn,
                a.QueueNo,
                a.CheckInAt,
                a.StartAt,
                a.EndAt))
            .ToListAsync(ct);
    }
}
