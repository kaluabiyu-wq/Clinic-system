namespace Clinic.Domain.Rules;


public static class ClinicTime
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(3);

    public static DateTime ToLocal(DateTime utc) =>
        new(utc.Ticks + Offset.Ticks, DateTimeKind.Unspecified);

    public static DateOnly Today(DateTime utcNow) =>
        DateOnly.FromDateTime(ToLocal(utcNow));
}
