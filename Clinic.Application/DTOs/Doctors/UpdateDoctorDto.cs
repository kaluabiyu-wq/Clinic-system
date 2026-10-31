namespace Clinic.Application.DTOs.Doctors;

/// <summary>QueueLetter is intentionally missing: it cannot change after creation.</summary>
public sealed record UpdateDoctorDto
{
    public required string FullName { get; init; }
    public required string Specialty { get; init; }
    public string? RoomNumber { get; init; }
    public required int AvgConsultMinutes { get; init; }
    public required bool IsActive { get; init; }
}
