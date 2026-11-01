using Clinic.Domain.Common;

namespace Clinic.Domain.Rules;

public static class SlotGenerator
{
    
    public static IReadOnlyList<TimeOnly> Generate(TimeOnly open, TimeOnly close, int slotMinutes)
    {
        if (slotMinutes <= 0)
            throw new DomainException("Slot length must be greater than zero.");

        if (close <= open)
            return Array.Empty<TimeOnly>();

       
        var step = TimeSpan.FromMinutes(slotMinutes);
        var end = close.ToTimeSpan();
        var slots = new List<TimeOnly>();

        for (var t = open.ToTimeSpan(); t + step <= end; t += step)
            slots.Add(TimeOnly.FromTimeSpan(t));

        return slots;
    }
}
