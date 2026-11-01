namespace Clinic.Application.DTOs.Schedules;

public sealed record SlotDto(
    TimeOnly StartTime,
    TimeOnly EndTime);
