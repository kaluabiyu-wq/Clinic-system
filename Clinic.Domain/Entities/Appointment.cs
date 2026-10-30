using Clinic.Domain.Common;
using Clinic.Domain.Enums;

namespace Clinic.Domain.Entities;

public class Appointment : BaseEntity
{
    /// <summary>Public booking reference such as APT1006. Unique.</summary>
    public required string Reference { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DateOnly Date { get; set; }

    /// <summary>Null for walk-ins, required for booked slots (enforced by a check constraint).</summary>
    public TimeOnly? SlotTime { get; set; }

    public bool IsWalkIn { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;

    /// <summary>Issued at check-in, per doctor per day. Display form comes from QueueNumberFormatter.</summary>
    public int? QueueNo { get; set; }

    public string? Reason { get; set; }
    public string? CancelReason { get; set; }

    public DateTime? CheckInAtUtc { get; set; }
    public DateTime? StartAtUtc { get; set; }
    public DateTime? EndAtUtc { get; set; }

    /// <summary>Maps to PostgreSQL xmin; EF compares it on UPDATE to detect lost updates.</summary>
    public uint Version { get; set; }

    /// <summary>Derived, never stored, so it cannot drift from the two timestamps.</summary>
    public TimeSpan? WaitTime => StartAtUtc - CheckInAtUtc;

    public ICollection<AppointmentEvent> Events { get; set; } = new List<AppointmentEvent>();
}
