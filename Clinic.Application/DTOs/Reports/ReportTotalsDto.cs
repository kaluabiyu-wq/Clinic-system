namespace Clinic.Application.DTOs.Reports;

/// <summary>
/// Total = Visits + Cancellations + NoShows + Open.
/// Visits are Completed consultations. Open means still Booked, Waiting or Serving.
/// WalkIns is how many of the appointments were walk-ins, whatever their outcome.
/// </summary>
public sealed record ReportTotalsDto(
    int Total,
    int Visits,
    int Cancellations,
    int NoShows,
    int WalkIns,
    int Open);
