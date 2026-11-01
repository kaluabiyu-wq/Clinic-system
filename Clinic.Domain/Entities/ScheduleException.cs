using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;


public class ScheduleException : BaseEntity
{
    public int DoctorId { get; set; }
    public DateOnly Date { get; set; }

    public string? Reason { get; set; }
}
