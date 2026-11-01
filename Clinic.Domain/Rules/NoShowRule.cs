using Clinic.Domain.Common;
using Clinic.Domain.Entities;
using Clinic.Domain.Enums;

namespace Clinic.Domain.Rules;

/// <summary>
/// FR14: a no-show can be recorded only once the slot is more than 10 minutes past
/// and the patient has not checked in.
/// </summary>
public static class NoShowRule
{
    public const int GraceMinutes = 10;

    /// <param name="clinicNow">Current clinic-local time (see ClinicTime).</param>
    public static bool CanMarkNoShow(Appointment appointment, DateTime clinicNow) =>
        appointment.Status == AppointmentStatus.Booked
        && !appointment.IsWalkIn
        && appointment.CheckInAt is null
        && clinicNow > appointment.Date.ToDateTime(appointment.SlotTime).AddMinutes(GraceMinutes);

    public static void EnsureCanMarkNoShow(Appointment appointment, DateTime clinicNow)
    {
        if (appointment.Status != AppointmentStatus.Booked || appointment.IsWalkIn || appointment.CheckInAt is not null)
            throw new DomainException("Only a booked appointment that has not been checked in can be marked as a no-show.");

        if (!CanMarkNoShow(appointment, clinicNow))
            throw new DomainException(
                $"A no-show can only be recorded more than {GraceMinutes} minutes after the appointment time.");
    }
}
