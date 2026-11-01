using Clinic.Domain.Common;
using Clinic.Domain.Enums;

namespace Clinic.Domain.Entities;
public class AppointmentEvent : BaseEntity
{
    public int AppointmentId { get; set; }

    public AppointmentStatus? FromStatus { get; set; }
    public AppointmentStatus ToStatus { get; set; }

    public string? Note { get; set; }
    public string? ChangedByUserId { get; set; }
}
