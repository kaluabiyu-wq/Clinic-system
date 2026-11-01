namespace Clinic.Application.DTOs.Queue;

public sealed record QueueBoardDto(
    DateTime UpdatedAtUtc,
    IReadOnlyList<QueueBoardCardDto> Doctors);
