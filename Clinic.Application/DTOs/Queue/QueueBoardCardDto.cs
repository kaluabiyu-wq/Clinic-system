namespace Clinic.Application.DTOs.Queue;

/// <summary>One doctor on the public board. Contains queue numbers only, never patient names.</summary>
public sealed record QueueBoardCardDto(
    int DoctorId,
    string DoctorName,
    string? RoomNumber,
    string QueueLetter,
    string? NowServing,
    IReadOnlyList<string> NextUp,
    int WaitingCount);
