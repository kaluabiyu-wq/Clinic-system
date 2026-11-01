namespace Clinic.Application.DTOs.Queue;

public sealed record CheckInResultDto(
    int AppointmentId,
    string Reference,
    string QueueNo,
    string DoctorName,
    string? RoomNumber,
    int PeopleAhead,            // patients who are waiting and arrived before this one
    int EstimatedWaitMinutes,   // rounded to the nearest 5 minutes
    DateTime CheckInAt);
