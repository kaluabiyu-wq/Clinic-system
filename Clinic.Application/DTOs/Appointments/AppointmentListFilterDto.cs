using Clinic.Domain.Enums;

namespace Clinic.Application.DTOs.Appointments;

/// <summary>Query string: GET /appointments?date=2026-11-02&amp;doctorId=1&amp;status=Booked</summary>
public sealed record AppointmentListFilterDto
{
    /// <summary>Defaults to today (clinic time).</summary>
    public DateOnly? Date { get; init; }
    public int? DoctorId { get; init; }
    public AppointmentStatus? Status { get; init; }
}
