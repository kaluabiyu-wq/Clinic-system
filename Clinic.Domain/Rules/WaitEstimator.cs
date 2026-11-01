namespace Clinic.Domain.Rules;

public static class WaitEstimator
{
    /// <summary>A consultation that has overrun is still assumed to need at least this many more minutes.</summary>
    public const int MinRemainingMinutes = 3;

    /// <summary>Estimates are shown to the nearest 5 minutes.</summary>
    public const int RoundToMinutes = 5;

    /// <summary>
    /// estimate = max(avg - minutesAlreadySpent, 3) + peopleAhead * avg, shown to the nearest 5 minutes.
    /// </summary>
    /// <param name="avgConsultMinutes">The doctor's average consultation length.</param>
    /// <param name="minutesAlreadySpent">
    /// Minutes the patient currently in consultation has already spent, or null if nobody is in
    /// consultation (then there is no remaining time to add for them).
    /// </param>
    /// <param name="peopleAhead">Patients who are waiting and arrived before this one.</param>
    public static int EstimateMinutes(int avgConsultMinutes, int? minutesAlreadySpent, int peopleAhead)
    {
        var remainingForCurrent = minutesAlreadySpent is null
            ? 0
            : Math.Max(avgConsultMinutes - minutesAlreadySpent.Value, MinRemainingMinutes);

        var estimate = remainingForCurrent + Math.Max(peopleAhead, 0) * avgConsultMinutes;

        return (int)(Math.Round(estimate / (double)RoundToMinutes, MidpointRounding.AwayFromZero) * RoundToMinutes);
    }
}
