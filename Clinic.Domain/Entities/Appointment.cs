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

    /// <summary>Walk-ins do not hold a slot, so they are excluded from the double-booking index.</summary>
    public bool IsWalkIn { get; set; }

    // Filled by the check-in and consultation steps (Week 8). Wait time = StartAt - CheckInAt.
    public string? QueueNo { get; set; }
    public DateTime? CheckInAt { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    /// <summary>Booking reference shown to the patient, e.g. APT1006. Derived from Id, not stored.</summary>
    public string Reference => ReferenceGenerator.Format(Id);

    /// <summary>Audit trail: one row per status change or reschedule.</summary>
    public ICollection<AppointmentEvent> Events { get; set; } = new List<AppointmentEvent>();
}
