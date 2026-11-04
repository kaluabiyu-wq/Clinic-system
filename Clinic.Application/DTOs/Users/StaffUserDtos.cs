namespace Clinic.Application.DTOs.Users;

public sealed record StaffUserDto(
    string Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    string? DoctorId,
    DateTime CreatedAtUtc);

public sealed record CreateStaffUserDto(
    string FullName,
    string Email,
    string Password,
    string Role,
    string? DoctorId);

public sealed record UpdateStaffUserDto(
    string FullName,
    string Role,
    bool IsActive,
    string? DoctorId,
    string? NewPassword);
