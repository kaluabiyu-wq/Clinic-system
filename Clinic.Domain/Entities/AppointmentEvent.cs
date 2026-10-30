using Clinic.Domain.Enums;

namespace Clinic.Domain.Entities;

/// <summary>
/// Append-only audit row written on every status change. Deliberately does not
/// inherit BaseEntity: it has no UpdatedAt because it is never updated.
/// </summary>
public class AppointmentEvent
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    /// <summary>Null on the creation event (nothing came before Booked).</summary>
    public AppointmentStatus? FromStatus { get; set; }
    public AppointmentStatus ToStatus { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    /// <summary>Identity user id of the actor; null for public self-service actions.</summary>
    public string? UserId { get; set; }
    public string? Note { get; set; }
}
