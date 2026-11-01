using Clinic.Domain.Common;
using Clinic.Domain.Enums;
using static Clinic.Domain.Enums.AppointmentStatus;

namespace Clinic.Domain.Rules;

public static class StatusTransitions
{
    private static readonly IReadOnlyDictionary<AppointmentStatus, AppointmentStatus[]> Allowed =
        new Dictionary<AppointmentStatus, AppointmentStatus[]>
        {
            [Booked] = new[] { Waiting, Cancelled, NoShow },
            [Waiting] = new[] { Serving, Cancelled },
            [Serving] = new[] { Completed },
            [Completed] = Array.Empty<AppointmentStatus>(),
            [Cancelled] = Array.Empty<AppointmentStatus>(),
            [NoShow] = Array.Empty<AppointmentStatus>()
        };

    public static bool CanTransition(AppointmentStatus from, AppointmentStatus to) =>
        Allowed.TryGetValue(from, out var next) && next.Contains(to);

    public static void EnsureCanTransition(AppointmentStatus from, AppointmentStatus to)
    {
        if (!CanTransition(from, to))
            throw new DomainException($"An appointment that is {from} cannot change to {to}.");
    }

    /// <summary>Cancelled and no-show appointments release their slot.</summary>
    public static bool HoldsSlot(AppointmentStatus status) =>
        status is not (Cancelled or NoShow);
}
