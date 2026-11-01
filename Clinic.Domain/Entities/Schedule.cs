using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;


public class Schedule : BaseEntity
{
    public int DoctorId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public int SlotMinutes { get; set; }
}
