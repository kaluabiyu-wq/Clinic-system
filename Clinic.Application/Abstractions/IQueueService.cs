using Clinic.Application.DTOs.Queue;

namespace Clinic.Application.Abstractions;

public interface IQueueService
{
    /// <summary>FR9: check a booked patient in and issue the queue number.</summary>
    Task<CheckInResultDto> CheckInAsync(int appointmentId, CancellationToken ct = default);

    /// <summary>FR12: register a patient who arrived without a booking and queue them at once.</summary>
    Task<CheckInResultDto> WalkInAsync(WalkInDto dto, CancellationToken ct = default);

    /// <summary>FR10: public board, no patient names.</summary>
    Task<QueueBoardDto> GetBoardAsync(CancellationToken ct = default);

    /// <summary>FR11: public tracking by booking reference, no patient names.</summary>
    Task<TrackQueueDto> TrackAsync(string reference, CancellationToken ct = default);
}
