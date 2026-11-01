namespace Clinic.Application.DTOs.Schedules;


public sealed record UpsertScheduleDto
{
    public required IReadOnlyList<ScheduleDayDto> Days { get; init; }
}
