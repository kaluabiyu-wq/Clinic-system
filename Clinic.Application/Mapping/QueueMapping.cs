using Clinic.Application.DTOs.Queue;
using Clinic.Domain.Entities;

namespace Clinic.Application.Mapping;

public static class QueueMapping
{
    /// <summary>Expects Patient to be loaded.</summary>
    public static QueueEntryDto ToQueueEntry(this Appointment a, DateTime utcNow)
    {
        int? minutesWaiting = null;

        if (a.CheckInAt is not null)
        {
            // Waiting: still waiting, so count to now. Serving: they waited until they were called.
            var until = a.StartAt ?? utcNow;
            minutesWaiting = (int)Math.Max(0, (until - a.CheckInAt.Value).TotalMinutes);
        }

        return new QueueEntryDto(
            a.Id,
            a.Reference,
            a.QueueNo ?? string.Empty,
            a.Patient?.FullName ?? string.Empty,
            a.IsWalkIn,
            a.SlotTime,
            a.CheckInAt,
            a.StartAt,
            minutesWaiting);
    }
}
