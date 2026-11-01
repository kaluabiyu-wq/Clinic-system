namespace Clinic.Application.DTOs.Schedules;

public sealed record ScheduleDto(
    int DoctorId,
    IReadOnlyList<ScheduleDayDto> Days);
