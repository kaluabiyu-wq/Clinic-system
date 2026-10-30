using Clinic.Application.DTOs.Patients;

namespace Clinic.Application.Abstractions;

public interface IPatientService
{
    Task<CreatePatientResultDto> CreateAsync(CreatePatientDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<PatientSearchResultDto>> SearchAsync(string? query, CancellationToken ct = default);
    Task<PatientDto> GetByIdAsync(int id, CancellationToken ct = default);
}
