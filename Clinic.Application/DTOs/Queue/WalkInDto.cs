using Clinic.Domain.Enums;

namespace Clinic.Application.DTOs.Queue;

public sealed record WalkInDto
{
    public required int DoctorId { get; init; }

    /// <summary>An already registered patient. Leave out to register a new one with the fields below.</summary>
    public int? PatientId { get; init; }

    public string? FullName { get; init; }
    public string? PhoneNumber { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public Sex? Sex { get; init; }
}
