namespace Clinic.Application.DTOs.Reports;

public sealed record DailyReportDto(
    DateOnly From,
    DateOnly To,
    ReportTotalsDto Totals,
    IReadOnlyList<DoctorReportRowDto> Doctors,
    IReadOnlyList<DailyReportRowDto> Days);
