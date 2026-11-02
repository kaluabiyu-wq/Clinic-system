using Clinic.Domain.Enums;

namespace Clinic.Application.Abstractions;

/// <summary>
/// One appointment, reduced to what reports need. It deliberately has no patient name or phone number,
/// so nothing built from it can leak patient identity.
/// </summary>
public sealed record ReportRow(
    int Id,
    int DoctorId,
    string DoctorName,
    DateOnly Date,
    TimeOnly SlotTime,
    AppointmentStatus Status,
    bool IsWalkIn,
    string? QueueNo,
    DateTime? CheckInAt,   // UTC
    DateTime? StartAt,     // UTC
    DateTime? EndAt);      // UTC
