namespace Clinic.Application.DTOs.Doctors;

/// <summary>Full record for the Administrator screens.</summary>
public sealed record DoctorDto(
    int Id,
    string FullName,
    string Specialty,
    string QueueLetter,
    string? RoomNumber,
    int AvgConsultMinutes,
    bool IsActive,
    DateTime CreatedAt);
