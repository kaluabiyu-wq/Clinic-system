using Clinic.Domain.Enums;

namespace Clinic.Application.DTOs.Appointments;

public sealed record AppointmentDto(
    int Id,
    string Reference,
    int DoctorId,
    string DoctorName,
    string? RoomNumber,
    int PatientId,
    string PatientName,
    string PatientPhone,
    DateOnly Date,
    TimeOnly SlotTime,
    AppointmentStatus Status,
    bool IsWalkIn,
    string? QueueNo,
    DateTime CreatedAt);
