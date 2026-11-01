using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Appointments;
using Clinic.Application.DTOs.Schedules;
using Clinic.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IBookingService _service;
    private readonly IValidator<BookAppointmentDto> _bookValidator;
    private readonly IValidator<RescheduleDto> _rescheduleValidator;
    private readonly IValidator<CancelDto> _cancelValidator;

    public AppointmentsController(
        IBookingService service,
        IValidator<BookAppointmentDto> bookValidator,
        IValidator<RescheduleDto> rescheduleValidator,
        IValidator<CancelDto> cancelValidator)
    {
        _service = service;
        _bookValidator = bookValidator;
        _rescheduleValidator = rescheduleValidator;
        _cancelValidator = cancelValidator;
    }

    /// <summary>Public: free slots for a doctor on a date: GET /doctors/1/slots?date=2026-11-02</summary>
    [HttpGet("~/doctors/{doctorId:int}/slots")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<SlotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSlots(int doctorId, [FromQuery] DateOnly? date, CancellationToken ct)
    {
        if (date is null)
        {
            ModelState.AddModelError("date", "date is required, in the format yyyy-MM-dd.");
            return ValidationProblem(ModelState);
        }

        return Ok(await _service.GetFreeSlotsAsync(doctorId, date.Value, ct));
    }

    /// <summary>
    /// Public: book a slot. Patients send their own details; staff may send patientId instead.
    /// Returns 409 if the slot was just taken.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Book([FromBody] BookAppointmentDto dto, CancellationToken ct)
    {
        var validation = await _bookValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var appointment = await _service.BookAsync(dto, ct);

        return CreatedAtAction(nameof(GetByReference), new { reference = appointment.Reference }, appointment);
    }

    /// <summary>Public: look up your appointment: GET /appointments/APT1006?phone=0911223344</summary>
    [HttpGet("{reference}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByReference(string reference, [FromQuery] string? phone, CancellationToken ct) =>
        Ok(await _service.GetByReferenceAsync(reference, phone, ct));

    /// <summary>Public (reference + phone) or staff: move a booked appointment to another slot.</summary>
    [HttpPut("{reference}/reschedule")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reschedule(string reference, [FromBody] RescheduleDto dto, CancellationToken ct)
    {
        var validation = await _rescheduleValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        return Ok(await _service.RescheduleAsync(reference, dto, ct));
    }

    /// <summary>Public (reference + phone) or staff: cancel. The body is optional for staff.</summary>
    [HttpPost("{reference}/cancel")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(string reference, [FromBody] CancelDto? dto, CancellationToken ct)
    {
        dto ??= new CancelDto();

        var validation = await _cancelValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        return Ok(await _service.CancelAsync(reference, dto, ct));
    }

    /// <summary>Receptionist, Administrator: appointments for a date: GET /appointments?date=2026-11-02&amp;doctorId=1</summary>
    [HttpGet]
    [Authorize(Roles = $"{RoleNames.Receptionist},{RoleNames.Administrator}")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] AppointmentListFilterDto filter, CancellationToken ct) =>
        Ok(await _service.ListAsync(filter, ct));
}
