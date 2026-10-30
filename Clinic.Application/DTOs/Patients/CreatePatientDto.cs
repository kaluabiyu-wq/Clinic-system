using Clinic.Domain.Enums;

namespace Clinic.Application.DTOs.Patients;

public sealed record CreatePatientDto
{
    public required string FullName { get; init; }
    public required string PhoneNumber { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required Sex Sex { get; init; }
    public string? Address { get; init; }

    /// <summary>
    /// Set to true after the receptionist has seen the duplicate warning
    /// and confirmed this is a different person.
    /// </summary>
    public bool ConfirmNotDuplicate { get; init; }
}
