using Clinic.Application.DTOs.Schedules;

namespace Clinic.Application.Abstractions;

public interface IScheduleService
{
    Task<ScheduleDto> GetScheduleAsync(int doctorId, CancellationToken ct = default);
    Task<ScheduleDto> UpsertScheduleAsync(int doctorId, UpsertScheduleDto dto, CancellationToken ct = default);

    /// <summary>Closures from today onward.</summary>
    Task<IReadOnlyList<ClosureDto>> GetClosuresAsync(int doctorId, CancellationToken ct = default);
    Task<ClosureDto> CreateClosureAsync(int doctorId, CreateClosureDto dto, CancellationToken ct = default);
    Task DeleteClosureAsync(int doctorId, int closureId, CancellationToken ct = default);

    /// <summary>
    /// Slots the schedule produces for a date, ignoring existing appointments.
    /// Empty if the doctor does not work that weekday or is closed that day.
    /// </summary>
    Task<IReadOnlyList<SlotDto>> PreviewSlotsAsync(int doctorId, DateOnly date, CancellationToken ct = default);
}
