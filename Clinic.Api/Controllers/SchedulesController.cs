using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Schedules;
using Clinic.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("doctors/{doctorId:int}")]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _service;
    private readonly IValidator<UpsertScheduleDto> _scheduleValidator;
    private readonly IValidator<CreateClosureDto> _closureValidator;

    public SchedulesController(
        IScheduleService service,
        IValidator<UpsertScheduleDto> scheduleValidator,
        IValidator<CreateClosureDto> closureValidator)
    {
        _service = service;
        _scheduleValidator = scheduleValidator;
        _closureValidator = closureValidator;
    }

    /// <summary>Public: a doctor's weekly working hours.</summary>
    [HttpGet("schedule")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchedule(int doctorId, CancellationToken ct) =>
        Ok(await _service.GetScheduleAsync(doctorId, ct));

    /// <summary>Administrator: replace the weekly schedule. Weekdays left out become days off.</summary>
    [HttpPut("schedule")]
    [Authorize(Roles = RoleNames.Administrator)]
    [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertSchedule(int doctorId, [FromBody] UpsertScheduleDto dto, CancellationToken ct)
    {
        var validation = await _scheduleValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        return Ok(await _service.UpsertScheduleAsync(doctorId, dto, ct));
    }

    /// <summary>Administrator: preview the slots a date produces: GET /doctors/1/schedule/preview?date=2026-11-02</summary>
    [HttpGet("schedule/preview")]
    [Authorize(Roles = RoleNames.Administrator)]
    [ProducesResponseType(typeof(IReadOnlyList<SlotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PreviewSlots(int doctorId, [FromQuery] DateOnly? date, CancellationToken ct)
    {
        if (date is null)
        {
            ModelState.AddModelError("date", "date is required, in the format yyyy-MM-dd.");
            return ValidationProblem(ModelState);
        }

        return Ok(await _service.PreviewSlotsAsync(doctorId, date.Value, ct));
    }

    /// <summary>Administrator: upcoming closures (leave, holidays).</summary>
    [HttpGet("closures")]
    [Authorize(Roles = RoleNames.Administrator)]
    [ProducesResponseType(typeof(IReadOnlyList<ClosureDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClosures(int doctorId, CancellationToken ct) =>
        Ok(await _service.GetClosuresAsync(doctorId, ct));

    /// <summary>Administrator: close the doctor for one full day.</summary>
    [HttpPost("closures")]
    [Authorize(Roles = RoleNames.Administrator)]
    [ProducesResponseType(typeof(ClosureDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateClosure(int doctorId, [FromBody] CreateClosureDto dto, CancellationToken ct)
    {
        var validation = await _closureValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var closure = await _service.CreateClosureAsync(doctorId, dto, ct);

        return CreatedAtAction(nameof(GetClosures), new { doctorId }, closure);
    }

    /// <summary>Administrator: remove a closure.</summary>
    [HttpDelete("closures/{closureId:int}")]
    [Authorize(Roles = RoleNames.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteClosure(int doctorId, int closureId, CancellationToken ct)
    {
        await _service.DeleteClosureAsync(doctorId, closureId, ct);
        return NoContent();
    }
}
