using Clinic.Domain.Common;
using Clinic.Domain.Entities;
using Clinic.Domain.Enums;

namespace Clinic.Domain.Rules;


public static class NoShowRule
{
    public const int GraceMinutes = 10;

   
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
