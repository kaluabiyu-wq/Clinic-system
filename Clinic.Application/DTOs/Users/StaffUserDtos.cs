namespace Clinic.Application.DTOs.Users;

public sealed record StaffUserDto(
    string Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    Guid? DoctorId,
    DateTime CreatedAtUtc);

public sealed record CreateStaffUserDto(
    string FullName,
    string Email,
    string Password,
    string Role,
    Guid? DoctorId);

public sealed record UpdateStaffUserDto(
    string FullName,
    string Role,
    bool IsActive,
    Guid? DoctorId,
    string? NewPassword);
