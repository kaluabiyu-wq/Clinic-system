using Clinic.Domain.Enums;

namespace Clinic.Domain.Entities;

public class AppointmentEvent
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public AppointmentStatus? FromStatus { get; set; }
    public AppointmentStatus ToStatus { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    /// <summary>Identity user id of the actor; null for public self-service actions.</summary>
    public string? UserId { get; set; }
    public string? Note { get; set; }
}
