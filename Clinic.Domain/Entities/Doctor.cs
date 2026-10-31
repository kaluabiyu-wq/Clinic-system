using Clinic.Domain.Common;
using Clinic.Domain.Constants;

namespace Clinic.Domain.Entities;

public class Doctor : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;

    public string QueueLetter { get; set; } = string.Empty;

    public string? RoomNumber { get; set; }

    public int AvgConsultMinutes { get; set; } = ClinicDefaults.AvgConsultMinutes;

    public bool IsActive { get; set; } = true;

    public string? UserId { get; set; }

    public Schedule? Schedule { get; set; }
    public ICollection<ScheduleException> Closures { get; set; } = new List<ScheduleException>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
