using Clinic.Domain.Enums;

namespace Clinic.Application.DTOs.Appointments;

public sealed record BookAppointmentDto
{
    public required int DoctorId { get; init; }

    /// <summary>Format yyyy-MM-dd.</summary>
    public required DateOnly Date { get; init; }

    /// <summary>Slot start time, format HH:mm:ss. Must be one of the free slots.</summary>
    public required TimeOnly SlotTime { get; init; }

    /// <summary>Staff only: book for an already registered patient. Leave out for self-service.</summary>
    public int? PatientId { get; init; }

    // Self-service: send these when PatientId is not sent.
    public string? FullName { get; init; }
    public string? PhoneNumber { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public Sex? Sex { get; init; }
}
