using Clinic.Domain.Common;
using Clinic.Domain.Enums;
using Clinic.Domain.Rules;

namespace Clinic.Domain.Entities;

public class Appointment : BaseEntity
{
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly SlotTime { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;

    
    public bool IsWalkIn { get; set; }

    
    public string? QueueNo { get; set; }
    public DateTime? CheckInAt { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    
    public string Reference => ReferenceGenerator.Format(Id);

    
    public ICollection<AppointmentEvent> Events { get; set; } = new List<AppointmentEvent>();
}
