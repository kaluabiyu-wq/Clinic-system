namespace Clinic.Domain.Enums;

/// <summary>
/// Single source of truth for the lifecycle. The roadmap's "Done" and "Completed"
/// inconsistency is resolved here: the value is Completed everywhere.
/// Persisted as text, so renaming a member requires a data migration.
/// </summary>
public enum AppointmentStatus
{
    Booked,
    Waiting,
    Serving,
    Completed,
    Cancelled,
    NoShow
}
