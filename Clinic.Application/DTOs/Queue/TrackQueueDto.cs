using Clinic.Domain.Enums;

namespace Clinic.Application.DTOs.Queue;

/// <summary>
/// Public "Track My Queue" view. No patient names or phone numbers.
/// QueueNo, PeopleAhead and EstimatedWaitMinutes are null until the patient has checked in today.
/// </summary>
public sealed record TrackQueueDto(
    string Reference,
    AppointmentStatus Status,
    DateOnly Date,
    TimeOnly SlotTime,
    string DoctorName,
    string? RoomNumber,
    string? QueueNo,
    string? NowServing,
    int? PeopleAhead,
    int? EstimatedWaitMinutes);
