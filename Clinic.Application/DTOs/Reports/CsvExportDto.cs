namespace Clinic.Application.DTOs.Reports;

public sealed record CsvExportDto(string FileName, byte[] Content);
