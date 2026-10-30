namespace Clinic.Application.DTOs.Patients;

/// <summary>
/// Created = true  -> Patient is filled.
/// Created = false -> nothing was saved; PossibleDuplicates lists matches (FR2).
/// </summary>
public sealed record CreatePatientResultDto(
    bool Created,
    PatientDto? Patient,
    IReadOnlyList<PatientSearchResultDto> PossibleDuplicates);
