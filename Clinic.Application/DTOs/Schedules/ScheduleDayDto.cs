namespace Clinic.Application.DTOs.Schedules;

public sealed record ScheduleDayDto(
    DayOfWeek DayOfWeek,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    int SlotMinutes);
