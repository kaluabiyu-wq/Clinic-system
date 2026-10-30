using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;

public class Doctor : BaseEntity
{
    public required string FullName { get; set; }
    public required string Specialty { get; set; }

    /// <summary>One letter that prefixes this doctor's queue numbers (G -> G005).</summary>
    public required string QueueLetter { get; set; }

    public string? RoomName { get; set; }

    /// <summary>Feeds the wait estimate. 15 matches ClinicDefaults; override per doctor.</summary>
    public int AvgConsultMinutes { get; set; } = 15;

    public bool IsActive { get; set; } = true;

    /// <summary>Identity user id when this doctor signs in as a Clinician. Null otherwise.</summary>
    public string? UserId { get; set; }

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public ICollection<ScheduleException> Exceptions { get; set; } = new List<ScheduleException>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
