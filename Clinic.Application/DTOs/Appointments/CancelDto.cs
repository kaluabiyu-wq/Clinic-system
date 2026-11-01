namespace Clinic.Application.DTOs.Appointments;

public sealed record CancelDto
{
    /// <summary>The patient's phone number. Required for patients, not for staff.</summary>
    public string? PhoneNumber { get; init; }

    public string? Reason { get; init; }
}
