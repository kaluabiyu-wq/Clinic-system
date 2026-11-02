namespace Clinic.Application.DTOs.Reports;

/// <summary>Averages are null when there was nothing to measure (for example no completed consultations).</summary>
public sealed record DoctorReportRowDto(
    int DoctorId,
    string DoctorName,
    int Total,
    int Visits,
    int Cancellations,
    int NoShows,
    int WalkIns,
    double? AverageWaitMinutes,
    double? AverageConsultMinutes);
