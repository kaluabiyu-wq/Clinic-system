using Clinic.Application.DTOs.Reports;

namespace Clinic.Application.Services;

/// <summary>The date range and doctor a report covers, after the defaults have been applied.</summary>
public readonly record struct ReportRange(DateOnly Start, DateOnly End, int? DoctorId)
{
    /// <summary>With no dates the range is today; with one date it is that single day.</summary>
    public static ReportRange Resolve(ReportFilterDto filter, DateOnly today)
    {
        var start = filter.From ?? filter.To ?? today;
        var end = filter.To ?? start;

        return new ReportRange(start, end, filter.DoctorId);
    }
}
