namespace Clinic.Application.DTOs.Appointments;

public sealed record RescheduleDto
{
    public required DateOnly NewDate { get; init; }
    public required TimeOnly NewSlotTime { get; init; }

    /// <summary>The patient's phone number. Required for patients, not for staff.</summary>
    public string? PhoneNumber { get; init; }
}
