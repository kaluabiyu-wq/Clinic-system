using Clinic.Application.DTOs.Users;

namespace Clinic.Application.Abstractions;

/// <summary>Port implemented in Infrastructure with ASP.NET Core Identity.</summary>
public interface IStaffUserStore
{
    Task<IReadOnlyList<StaffUserDto>> ListAsync(CancellationToken ct);
    Task<StaffUserDto?> FindAsync(string id, CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);
    Task<int> CountActiveAdministratorsAsync(CancellationToken ct);
    Task<StaffUserDto> CreateAsync(CreateStaffUserDto dto, CancellationToken ct);
    Task<StaffUserDto> UpdateAsync(string id, UpdateStaffUserDto dto, CancellationToken ct);
}
