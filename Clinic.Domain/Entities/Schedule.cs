using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;

/// <summary>Weekly working block: one row per doctor per weekday.</summary>
public class Schedule : BaseEntity
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotMinutes { get; set; } = 15;
    public bool IsActive { get; set; } = true;
}
