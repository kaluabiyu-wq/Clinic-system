namespace Clinic.Application.DTOs.Reports;

/// <summary>
/// Wait = time from check-in to being called (StartAt - CheckInAt), for patients who were called.
/// The three measures are null when no patient in the range has a measured wait.
/// </summary>
public sealed record WaitReportDto(
    DateOnly From,
    DateOnly To,
    int MeasuredPatients,
    double? AverageWaitMinutes,
    int? LongestWaitMinutes,
    double? ShareWithin15MinutesPercent);
