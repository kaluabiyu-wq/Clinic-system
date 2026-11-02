namespace Clinic.Application.DTOs.Reports;

/// <summary>One calendar day of the report. Days with no appointments are included with zeros.</summary>
public sealed record DailyReportRowDto(
    DateOnly Date,
    int Visits,
    int Cancellations,
    int NoShows,
    int WalkIns);
