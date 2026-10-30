using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;


public class ScheduleException : BaseEntity
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DateOnly Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Reason { get; set; }
}
