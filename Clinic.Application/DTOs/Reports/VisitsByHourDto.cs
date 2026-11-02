namespace Clinic.Application.DTOs.Reports;

/// <summary>Completed visits that started in this clock hour (clinic local time, 0-23).</summary>
public sealed record VisitsByHourDto(int Hour, int Visits);
