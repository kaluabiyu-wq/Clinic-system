using Clinic.Application.DTOs.Schedules;
using Clinic.Domain.Entities;

namespace Clinic.Application.Mapping;

public static class ScheduleMapping
{
    public static ScheduleDayDto ToDayDto(this Schedule s) =>
        new(s.DayOfWeek, s.OpenTime, s.CloseTime, s.SlotMinutes);

   
    public static ScheduleDto ToScheduleDto(this IEnumerable<Schedule> schedules, int doctorId) =>
        new(doctorId, schedules.OrderBy(s => MondayFirst(s.DayOfWeek)).Select(s => s.ToDayDto()).ToList());

    public static ClosureDto ToDto(this ScheduleException c) =>
        new(c.Id, c.Date, c.Reason);

    private static int MondayFirst(DayOfWeek day) => ((int)day + 6) % 7;
}
