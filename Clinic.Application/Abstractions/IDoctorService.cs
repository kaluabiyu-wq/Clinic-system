using Clinic.Application.DTOs.Doctors;

namespace Clinic.Application.Abstractions;

public interface IDoctorService
{
    /// <summary>Public list for booking: active doctors only.</summary>
    Task<IReadOnlyList<DoctorSummaryDto>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>Administrator list: includes inactive doctors.</summary>
    Task<IReadOnlyList<DoctorDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Public view of one active doctor. Throws NotFoundException if missing or inactive.</summary>
    Task<DoctorSummaryDto> GetSummaryByIdAsync(int id, CancellationToken ct = default);

    Task<DoctorDto> CreateAsync(CreateDoctorDto dto, CancellationToken ct = default);
    Task<DoctorDto> UpdateAsync(int id, UpdateDoctorDto dto, CancellationToken ct = default);
}
