using Clinic.Domain.Common;
using Clinic.Domain.Enums;

namespace Clinic.Domain.Entities;

public class Appointment : BaseEntity
{
    public required string Reference { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DateOnly Date { get; set; }

    public TimeOnly? SlotTime { get; set; }

    public bool IsWalkIn { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;

    public int? QueueNo { get; set; }

    public string? Reason { get; set; }
    public string? CancelReason { get; set; }

    public DateTime? CheckInAtUtc { get; set; }
    public DateTime? StartAtUtc { get; set; }
    public DateTime? EndAtUtc { get; set; }

    public uint Version { get; set; }

    public TimeSpan? WaitTime => StartAtUtc - CheckInAtUtc;

    public ICollection<AppointmentEvent> Events { get; set; } = new List<AppointmentEvent>();
}
