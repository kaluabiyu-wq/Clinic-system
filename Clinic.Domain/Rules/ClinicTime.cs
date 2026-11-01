namespace Clinic.Domain.Rules;

/// <summary>
/// The clinic works in Ethiopian local time (East Africa Time, UTC+3, no daylight saving).
/// Appointment dates and times are stored in clinic-local time; audit timestamps stay UTC.
/// A fixed offset avoids time-zone id differences between Windows and Linux servers.
/// </summary>
public static class ClinicTime
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(3);

    public static DateTime ToLocal(DateTime utc) =>
        new(utc.Ticks + Offset.Ticks, DateTimeKind.Unspecified);

    public static DateOnly Today(DateTime utcNow) =>
        DateOnly.FromDateTime(ToLocal(utcNow));
}
