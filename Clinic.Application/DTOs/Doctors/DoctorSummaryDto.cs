namespace Clinic.Application.DTOs.Doctors;

/// <summary>Public view used by the booking page: only what a patient needs to pick a doctor.</summary>
public sealed record DoctorSummaryDto(
    int Id,
    string FullName,
    string Specialty,
    string? RoomNumber);
