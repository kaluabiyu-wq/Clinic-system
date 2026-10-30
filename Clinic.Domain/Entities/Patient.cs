using Clinic.Domain.Common;
using Clinic.Domain.Enums;

namespace Clinic.Domain.Entities;

public class Patient : BaseEntity
{
    public required string FullName { get; set; }
    public Sex Sex { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public required string Phone { get; set; }
    public string? Address { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
