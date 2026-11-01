using Clinic.Application.DTOs.Appointments;
using Clinic.Application.DTOs.Schedules;

namespace Clinic.Application.Abstractions;

public interface IBookingService
{
    /// <summary>Free slots for a date: doctor works that day, is not closed, slot not in the past, not already held.</summary>
    Task<IReadOnlyList<SlotDto>> GetFreeSlotsAsync(int doctorId, DateOnly date, CancellationToken ct = default);

    Task<AppointmentDto> BookAsync(BookAppointmentDto dto, CancellationToken ct = default);

    /// <summary>Patients prove ownership with reference + phone number; staff do not need the phone.</summary>
    Task<AppointmentDto> GetByReferenceAsync(string reference, string? phoneNumber, CancellationToken ct = default);

    Task<AppointmentDto> RescheduleAsync(string reference, RescheduleDto dto, CancellationToken ct = default);
    Task<AppointmentDto> CancelAsync(string reference, CancelDto dto, CancellationToken ct = default);

    /// <summary>Staff list for one date (default today), optionally by doctor and status.</summary>
    Task<IReadOnlyList<AppointmentDto>> ListAsync(AppointmentListFilterDto filter, CancellationToken ct = default);
}
