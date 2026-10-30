namespace Clinic.Application.DTOs.Patients;

public sealed record PatientSearchResultDto(
    int Id,
    string FullName,
    string PhoneNumber,
    DateOnly DateOfBirth);
