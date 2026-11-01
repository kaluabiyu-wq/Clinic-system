using Clinic.Application.DTOs.Appointments;
using Clinic.Application.DTOs.Queue;

namespace Clinic.Application.Abstractions;

public interface IAttendanceService
{
    /// <summary>The doctor's room view: who is in consultation and who is waiting.</summary>
    Task<ConsultationRoomDto> GetRoomAsync(int doctorId, CancellationToken ct = default);

    /// <summary>FR13: call the next waiting patient (Waiting to Serving) and stamp StartAt.</summary>
    Task<ConsultationRoomDto> CallNextAsync(int doctorId, CancellationToken ct = default);

    /// <summary>FR13: finish the consultation (Serving to Completed) and stamp EndAt.</summary>
    Task<AppointmentDto> CompleteAsync(int appointmentId, CancellationToken ct = default);

    /// <summary>FR14: record a no-show, only allowed more than 10 minutes after the slot with no check-in.</summary>
    Task<AppointmentDto> MarkNoShowAsync(int appointmentId, CancellationToken ct = default);
}
