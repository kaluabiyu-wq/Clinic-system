namespace Clinic.Application.DTOs.Reports;

/// <summary>Query string: GET /reports/daily?from=2026-11-01&amp;to=2026-11-30&amp;doctorId=1</summary>
public sealed record ReportFilterDto
{
    /// <summary>Format yyyy-MM-dd. Defaults to To, or to today if neither is sent.</summary>
    public DateOnly? From { get; init; }

    /// <summary>Format yyyy-MM-dd. Defaults to From.</summary>
    public DateOnly? To { get; init; }

    /// <summary>Leave out for all doctors.</summary>
    public int? DoctorId { get; init; }
}
