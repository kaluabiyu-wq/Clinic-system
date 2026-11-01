namespace Clinic.Application.DTOs.Queue;

/// <summary>A patient in a doctor's queue. Staff only: it contains the patient's name.</summary>
public sealed record QueueEntryDto(
    int AppointmentId,
    string Reference,
    string QueueNo,
    string PatientName,
    bool IsWalkIn,
    TimeOnly SlotTime,
    DateTime? CheckInAt,
    DateTime? StartAt,
    int? MinutesWaiting);   // waiting: minutes since check-in; in consultation: minutes they waited before being called
