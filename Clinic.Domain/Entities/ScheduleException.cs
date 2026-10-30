using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;

/// <summary>
/// A closure on one date. StartTime and EndTime both null means the whole day;
/// both set means only that window is closed (for example a 10:00-12:00 meeting).
/// </summary>
public class ScheduleException : BaseEntity
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DateOnly Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Reason { get; set; }
}
