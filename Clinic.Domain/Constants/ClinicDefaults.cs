namespace Clinic.Domain.Constants;

public static class ClinicDefaults
{
    /// <summary>Starting average consultation length, used by the wait estimator until real data exists.</summary>
    public const int AvgConsultMinutes = 10;

    public const int MinConsultMinutes = 1;
    public const int MaxConsultMinutes = 120;
}
