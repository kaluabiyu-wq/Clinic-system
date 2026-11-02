namespace Clinic.Application.Abstractions;

public interface IReportRepository
{
    /// <summary>All appointments with a Date between from and to (inclusive), optionally for one doctor.</summary>
    Task<IReadOnlyList<ReportRow>> GetRowsAsync(
        DateOnly from, DateOnly to, int? doctorId, CancellationToken ct = default);
}
