namespace Clinic.Domain.Constants;

public static class RoleNames
{
    public const string Receptionist = "Receptionist";
    public const string Clinician = "Clinician";
    public const string Administrator = "Administrator";

    public static readonly string[] All = { Receptionist, Clinician, Administrator };
}
