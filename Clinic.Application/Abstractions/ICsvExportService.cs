using Clinic.Application.DTOs.Reports;

namespace Clinic.Application.Abstractions;

public interface ICsvExportService
{
    /// <summary>FR17: one CSV row per appointment. Never contains patient names or phone numbers.</summary>
    Task<CsvExportDto> ExportAsync(ReportFilterDto filter, CancellationToken ct = default);
}
