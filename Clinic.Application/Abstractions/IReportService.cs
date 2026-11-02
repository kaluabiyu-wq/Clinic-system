using Clinic.Application.DTOs.Reports;

namespace Clinic.Application.Abstractions;

public interface IReportService
{
    /// <summary>FR15: visits, cancellations and no-shows, per doctor and per day.</summary>
    Task<DailyReportDto> GetDailyAsync(ReportFilterDto filter, CancellationToken ct = default);

    /// <summary>FR16: average and longest wait, and the share who waited 15 minutes or less.</summary>
    Task<WaitReportDto> GetWaitsAsync(ReportFilterDto filter, CancellationToken ct = default);

    /// <summary>Completed visits for each hour of the clinic day (0-23).</summary>
    Task<IReadOnlyList<VisitsByHourDto>> GetVisitsByHourAsync(ReportFilterDto filter, CancellationToken ct = default);
}
