using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Reports;
using Clinic.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

/// <summary>
/// Administrator reports. Every endpoint takes the same optional query string:
/// ?from=2026-11-01&amp;to=2026-11-30&amp;doctorId=1 (no dates means today).
/// </summary>
[ApiController]
[Route("reports")]
[Authorize(Roles = RoleNames.Administrator)]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;
    private readonly ICsvExportService _csv;
    private readonly IValidator<ReportFilterDto> _validator;

    public ReportsController(IReportService reports, ICsvExportService csv, IValidator<ReportFilterDto> validator)
    {
        _reports = reports;
        _csv = csv;
        _validator = validator;
    }

    /// <summary>Visits, cancellations, no-shows and walk-ins: totals, per doctor and per day.</summary>
    [HttpGet("daily")]
    [ProducesResponseType(typeof(DailyReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Daily([FromQuery] ReportFilterDto filter, CancellationToken ct)
    {
        if (await InvalidAsync(filter, ct) is { } invalid)
            return invalid;

        return Ok(await _reports.GetDailyAsync(filter, ct));
    }

    /// <summary>Average and longest wait, and the share of patients who waited 15 minutes or less.</summary>
    [HttpGet("waits")]
    [ProducesResponseType(typeof(WaitReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Waits([FromQuery] ReportFilterDto filter, CancellationToken ct)
    {
        if (await InvalidAsync(filter, ct) is { } invalid)
            return invalid;

        return Ok(await _reports.GetWaitsAsync(filter, ct));
    }

    /// <summary>Completed visits for each hour of the day (0-23).</summary>
    [HttpGet("visits-by-hour")]
    [ProducesResponseType(typeof(IReadOnlyList<VisitsByHourDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VisitsByHour([FromQuery] ReportFilterDto filter, CancellationToken ct)
    {
        if (await InvalidAsync(filter, ct) is { } invalid)
            return invalid;

        return Ok(await _reports.GetVisitsByHourAsync(filter, ct));
    }

    /// <summary>CSV download, one row per appointment, without patient names or phone numbers.</summary>
    [HttpGet("export.csv")]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportCsv([FromQuery] ReportFilterDto filter, CancellationToken ct)
    {
        if (await InvalidAsync(filter, ct) is { } invalid)
            return invalid;

        var file = await _csv.ExportAsync(filter, ct);

        return File(file.Content, "text/csv; charset=utf-8", file.FileName);
    }

    private async Task<IActionResult?> InvalidAsync(ReportFilterDto filter, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(filter, ct);

        return validation.IsValid
            ? null
            : ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
    }
}
