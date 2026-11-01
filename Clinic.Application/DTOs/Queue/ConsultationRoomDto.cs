namespace Clinic.Application.DTOs.Queue;

public sealed record ConsultationRoomDto(
    int DoctorId,
    string DoctorName,
    string? RoomNumber,
    QueueEntryDto? Current,
    IReadOnlyList<QueueEntryDto> Waiting);
