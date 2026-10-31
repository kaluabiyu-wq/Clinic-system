namespace Clinic.Application.DTOs.Doctors;

public sealed record CreateDoctorDto
{
    public required string FullName { get; init; }
    public required string Specialty { get; init; }

    /// <summary>A single letter A-Z. Cannot be changed later.</summary>
    public required string QueueLetter { get; init; }

    public string? RoomNumber { get; init; }

    /// <summary>Optional. Defaults to ClinicDefaults.AvgConsultMinutes.</summary>
    public int? AvgConsultMinutes { get; init; }
}
