using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Reports;
using Clinic.Domain.Common;
using Clinic.Domain.Rules;

namespace Clinic.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reports;
    private readonly IDoctorRepository _doctors;
    private readonly IClock _clock;

    public ReportService(IReportRepository reports, IDoctorRepository doctors, IClock clock)
    {
        _reports = reports;
        _doctors = doctors;
        _clock = clock;
    }

    public async Task<DailyReportDto> GetDailyAsync(ReportFilterDto filter, CancellationToken ct = default)
    {
        var (range, rows) = await LoadAsync(filter, ct);

        return new DailyReportDto(
            range.Start,
            range.End,
            ReportCalculator.Totals(rows),
            ReportCalculator.PerDoctor(rows),
            ReportCalculator.PerDay(rows, range.Start, range.End));
    }

    public async Task<WaitReportDto> GetWaitsAsync(ReportFilterDto filter, CancellationToken ct = default)
    {
        var (range, rows) = await LoadAsync(filter, ct);
        return ReportCalculator.Waits(rows, range.Start, range.End);
    }

    public async Task<IReadOnlyList<VisitsByHourDto>> GetVisitsByHourAsync(ReportFilterDto filter, CancellationToken ct = default)
    {
        var (_, rows) = await LoadAsync(filter, ct);
        return ReportCalculator.VisitsByHour(rows);
    }

    private async Task<(ReportRange Range, IReadOnlyList<ReportRow> Rows)> LoadAsync(ReportFilterDto filter, CancellationToken ct)
    {
        var range = ReportRange.Resolve(filter, ClinicTime.Today(_clock.UtcNow));

        if (range.DoctorId is not null && await _doctors.GetByIdAsync(range.DoctorId.Value, ct) is null)
            throw new NotFoundException($"Doctor {range.DoctorId} was not found.");

        var rows = await _reports.GetRowsAsync(range.Start, range.End, range.DoctorId, ct);

        return (range, rows);
    }
}
