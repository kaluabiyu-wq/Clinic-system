using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Reports;
using Clinic.Domain.Enums;
using Clinic.Domain.Rules;

namespace Clinic.Application.Services;

/// <summary>
/// The report arithmetic, with no database or clock, so it can be unit tested with plain lists of rows.
/// </summary>
public static class ReportCalculator
{
    /// <summary>FR16: patients who waited this many minutes or less count as "seen quickly".</summary>
    public const int QuickWaitThresholdMinutes = 15;

    public static ReportTotalsDto Totals(IEnumerable<ReportRow> rows)
    {
        var list = rows as IReadOnlyCollection<ReportRow> ?? rows.ToList();

        return new ReportTotalsDto(
            Total: list.Count,
            Visits: list.Count(r => r.Status == AppointmentStatus.Completed),
            Cancellations: list.Count(r => r.Status == AppointmentStatus.Cancelled),
            NoShows: list.Count(r => r.Status == AppointmentStatus.NoShow),
            WalkIns: list.Count(r => r.IsWalkIn),
            Open: list.Count(r => IsOpen(r.Status)));
    }

    public static IReadOnlyList<DoctorReportRowDto> PerDoctor(IEnumerable<ReportRow> rows) =>
        rows.GroupBy(r => new { r.DoctorId, r.DoctorName })
            .OrderBy(g => g.Key.DoctorName)
            .Select(g =>
            {
                var items = g.ToList();
                var totals = Totals(items);

                return new DoctorReportRowDto(
                    g.Key.DoctorId,
                    g.Key.DoctorName,
                    totals.Total,
                    totals.Visits,
                    totals.Cancellations,
                    totals.NoShows,
                    totals.WalkIns,
                    AverageOrNull(items.Select(WaitMinutes)),
                    AverageOrNull(items.Select(ConsultMinutes)));
            })
            .ToList();

    /// <summary>One row for every day from start to end, including days with nothing.</summary>
    public static IReadOnlyList<DailyReportRowDto> PerDay(IEnumerable<ReportRow> rows, DateOnly start, DateOnly end)
    {
        var byDate = rows
            .GroupBy(r => r.Date)
            .ToDictionary(g => g.Key, g => Totals(g.ToList()));

        var days = new List<DailyReportRowDto>();

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            byDate.TryGetValue(date, out var totals);

            days.Add(new DailyReportRowDto(
                date,
                totals?.Visits ?? 0,
                totals?.Cancellations ?? 0,
                totals?.NoShows ?? 0,
                totals?.WalkIns ?? 0));
        }

        return days;
    }

    public static WaitReportDto Waits(IEnumerable<ReportRow> rows, DateOnly start, DateOnly end)
    {
        var waits = rows
            .Select(WaitMinutes)
            .Where(w => w.HasValue)
            .Select(w => w!.Value)
            .ToList();

        if (waits.Count == 0)
            return new WaitReportDto(start, end, 0, null, null, null);

        var quick = waits.Count(w => w <= QuickWaitThresholdMinutes);

        return new WaitReportDto(
            start,
            end,
            waits.Count,
            Round1(waits.Average()),
            (int)Math.Round(waits.Max(), MidpointRounding.AwayFromZero),
            Round1(100.0 * quick / waits.Count));
    }

    /// <summary>Completed visits by the clinic-local hour the consultation started. Always 24 entries.</summary>
    public static IReadOnlyList<VisitsByHourDto> VisitsByHour(IEnumerable<ReportRow> rows)
    {
        var counts = new int[24];

        foreach (var row in rows.Where(r => r.Status == AppointmentStatus.Completed && r.StartAt.HasValue))
            counts[ClinicTime.ToLocal(row.StartAt!.Value).Hour]++;

        return Enumerable.Range(0, 24)
            .Select(hour => new VisitsByHourDto(hour, counts[hour]))
            .ToList();
    }

    /// <summary>Minutes from check-in to being called, or null if the patient was never called.</summary>
    public static double? WaitMinutes(ReportRow row) =>
        row.CheckInAt is { } checkIn && row.StartAt is { } start && start >= checkIn
            ? (start - checkIn).TotalMinutes
            : null;

    /// <summary>Minutes from the start to the end of the consultation, or null if it has not finished.</summary>
    public static double? ConsultMinutes(ReportRow row) =>
        row.StartAt is { } start && row.EndAt is { } end && end >= start
            ? (end - start).TotalMinutes
            : null;

    private static bool IsOpen(AppointmentStatus status) =>
        status is AppointmentStatus.Booked or AppointmentStatus.Waiting or AppointmentStatus.Serving;

    private static double? AverageOrNull(IEnumerable<double?> values)
    {
        var list = values.Where(v => v.HasValue).Select(v => v!.Value).ToList();
        return list.Count == 0 ? null : Round1(list.Average());
    }

    private static double Round1(double value) => Math.Round(value, 1, MidpointRounding.AwayFromZero);
}
