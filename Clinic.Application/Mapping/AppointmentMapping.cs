using Clinic.Application.DTOs.Appointments;
using Clinic.Domain.Entities;

namespace Clinic.Application.Mapping;

public static class AppointmentMapping
{
    /// <summary>Expects Doctor and Patient to be loaded.</summary>
    public static AppointmentDto ToDto(this Appointment a) =>
        new(
            a.Id,
            a.Reference,
            a.DoctorId,
            a.Doctor?.FullName ?? string.Empty,
            a.Doctor?.RoomNumber,
            a.PatientId,
            a.Patient?.FullName ?? string.Empty,
            a.Patient?.PhoneNumber ?? string.Empty,
            a.Date,
            a.SlotTime,
            a.Status,
            a.IsWalkIn,
            a.QueueNo,
            a.CreatedAt);
}
