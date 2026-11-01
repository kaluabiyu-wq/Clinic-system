using Clinic.Domain.Entities;

namespace Clinic.Domain.Rules;

public enum SlotCheckResult
{
    Available,
    DoctorClosed,
    DoctorNotWorking,
    NotASlot,
    InPast,
    Taken
}

/// <summary>
/// FR5: a slot is free when the doctor works that weekday, is not closed that date,
/// the slot is not in the past, and no active appointment holds it.
/// </summary>
public static class SlotAvailabilityChecker
{
    /// <param name="workingDay">The doctor's Schedule row for that weekday, or null for a day off.</param>
    /// <param name="takenSlots">Start times held by active, non-walk-in appointments on that date.</param>
    /// <param name="clinicNow">Current clinic-local time (see ClinicTime).</param>
    public static SlotCheckResult Check(
        Schedule? workingDay,
        bool isClosed,
        DateOnly date,
        TimeOnly slot,
        IReadOnlyCollection<TimeOnly> takenSlots,
        DateTime clinicNow)
    {
        if (isClosed)
            return SlotCheckResult.DoctorClosed;

        if (workingDay is null)
            return SlotCheckResult.DoctorNotWorking;

        var slots = SlotGenerator.Generate(workingDay.OpenTime, workingDay.CloseTime, workingDay.SlotMinutes);
        if (!slots.Contains(slot))
            return SlotCheckResult.NotASlot;

        if (date.ToDateTime(slot) <= clinicNow)
            return SlotCheckResult.InPast;

        if (takenSlots.Contains(slot))
            return SlotCheckResult.Taken;

        return SlotCheckResult.Available;
    }

    public static IReadOnlyList<TimeOnly> GetFreeSlots(
        Schedule? workingDay,
        bool isClosed,
        DateOnly date,
        IReadOnlyCollection<TimeOnly> takenSlots,
        DateTime clinicNow)
    {
        if (isClosed || workingDay is null)
            return Array.Empty<TimeOnly>();

        return SlotGenerator.Generate(workingDay.OpenTime, workingDay.CloseTime, workingDay.SlotMinutes)
            .Where(s => date.ToDateTime(s) > clinicNow && !takenSlots.Contains(s))
            .ToList();
    }
}
