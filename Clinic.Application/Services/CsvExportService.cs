using System.Globalization;
using System.Text;
using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Reports;
using Clinic.Domain.Common;
using Clinic.Domain.Rules;

namespace Clinic.Application.Services;

public class CsvExportService : ICsvExportService
{
    private static readonly string[] Headers =
    {
        "Date", "Reference", "QueueNo", "Doctor", "Type", "SlotTime", "Status",
        "CheckIn", "ServiceStart", "ServiceEnd", "WaitMinutes", "ConsultMinutes"
    };

    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: true);

    private readonly IReportRepository _reports;
    private readonly IDoctorRepository _doctors;
    private readonly IClock _clock;

    public CsvExportService(IReportRepository reports, IDoctorRepository doctors, IClock clock)
    {
        _reports = reports;
        _doctors = doctors;
        _clock = clock;
    }

    public async Task<CsvExportDto> ExportAsync(ReportFilterDto filter, CancellationToken ct = default)
    {
        var range = ReportRange.Resolve(filter, ClinicTime.Today(_clock.UtcNow));

        if (range.DoctorId is not null && await _doctors.GetByIdAsync(range.DoctorId.Value, ct) is null)
            throw new NotFoundException($"Doctor {range.DoctorId} was not found.");

        var rows = await _reports.GetRowsAsync(range.Start, range.End, range.DoctorId, ct);

        var csv = new StringBuilder();
        csv.Append(string.Join(',', Headers)).Append("\r\n");

        foreach (var row in rows)
        {
            var wait = ReportCalculator.WaitMinutes(row);
            var consult = ReportCalculator.ConsultMinutes(row);

            // Patient names and phone numbers are never exported: the reference and queue number identify the visit.
            var fields = new[]
            {
                row.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ReferenceGenerator.Format(row.Id),
                row.QueueNo,
                row.DoctorName,
                row.IsWalkIn ? "Walk-in" : "Booked",
                row.SlotTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                row.Status.ToString(),
                LocalTime(row.CheckInAt),
                LocalTime(row.StartAt),
                LocalTime(row.EndAt),
                Minutes(wait),
                Minutes(consult)
            };

            csv.Append(string.Join(',', fields.Select(Escape))).Append("\r\n");
        }

        var preamble = Utf8.GetPreamble();
        var body = Utf8.GetBytes(csv.ToString());
        var content = new byte[preamble.Length + body.Length];
        preamble.CopyTo(content, 0);
        body.CopyTo(content, preamble.Length);

        return new CsvExportDto(
            $"clinic-report-{range.Start:yyyy-MM-dd}-to-{range.End:yyyy-MM-dd}.csv",
            content);
    }

    /// <summary>
    /// Quotes a field when it contains a comma, quote or line break. Text that starts with = + - @ is
    /// prefixed with an apostrophe so a spreadsheet cannot run it as a formula (CSV injection).
    /// </summary>
    public static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
            value = "'" + value;

        if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0)
            value = "\"" + value.Replace("\"", "\"\"") + "\"";

        return value;
    }

    private static string? LocalTime(DateTime? utc) =>
        utc is null ? null : ClinicTime.ToLocal(utc.Value).ToString("HH:mm", CultureInfo.InvariantCulture);

    private static string? Minutes(double? minutes) =>
        minutes is null ? null : minutes.Value.ToString("0.0", CultureInfo.InvariantCulture);
}
