using Clinic.Domain.Enums;

namespace Clinic.Application.DTOs.Patients;

public sealed record PatientDto(
    int Id,
    string FullName,
    string PhoneNumber,
    DateOnly DateOfBirth,
    Sex Sex,
    string? Address,
    DateTime CreatedAt);
