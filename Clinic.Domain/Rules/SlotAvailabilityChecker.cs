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


public static class SlotAvailabilityChecker
{
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
