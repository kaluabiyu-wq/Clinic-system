namespace Clinic.Domain.Rules;

public static class WaitEstimator
{
    
    public const int MinRemainingMinutes = 3;

    
    public const int RoundToMinutes = 5;

        public static int EstimateMinutes(int avgConsultMinutes, int? minutesAlreadySpent, int peopleAhead)
    {
        var remainingForCurrent = minutesAlreadySpent is null
            ? 0
            : Math.Max(avgConsultMinutes - minutesAlreadySpent.Value, MinRemainingMinutes);

        var estimate = remainingForCurrent + Math.Max(peopleAhead, 0) * avgConsultMinutes;

        return (int)(Math.Round(estimate / (double)RoundToMinutes, MidpointRounding.AwayFromZero) * RoundToMinutes);
    }
}
