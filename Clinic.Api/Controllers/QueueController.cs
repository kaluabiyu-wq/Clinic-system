using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Appointments;
using Clinic.Application.DTOs.Queue;
using Clinic.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

/// <summary>Staff queue actions: reception (check-in, walk-in, no-show) and clinicians (call next, complete).</summary>
[ApiController]
public class QueueController : ControllerBase
{
    private const string ReceptionRoles = $"{RoleNames.Receptionist},{RoleNames.Administrator}";
    private const string ClinicianRoles = $"{RoleNames.Clinician},{RoleNames.Administrator}";

    private readonly IQueueService _queue;
    private readonly IAttendanceService _attendance;
    private readonly IValidator<WalkInDto> _walkInValidator;

    public QueueController(
        IQueueService queue,
        IAttendanceService attendance,
        IValidator<WalkInDto> walkInValidator)
    {
        _queue = queue;
        _attendance = attendance;
        _walkInValidator = walkInValidator;
    }

    /// <summary>Reception: check a booked patient in and issue the queue number.</summary>
    [HttpPost("/appointments/{id:int}/check-in")]
    [Authorize(Roles = ReceptionRoles)]
    [ProducesResponseType(typeof(CheckInResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckIn(int id, CancellationToken ct) =>
        Ok(await _queue.CheckInAsync(id, ct));

    /// <summary>Reception: register a walk-in and queue them at once.</summary>
    [HttpPost("/walk-ins")]
    [Authorize(Roles = ReceptionRoles)]
    [ProducesResponseType(typeof(CheckInResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> WalkIn([FromBody] WalkInDto dto, CancellationToken ct)
    {
        var validation = await _walkInValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await _queue.WalkInAsync(dto, ct);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Reception: record a no-show. Allowed only more than 10 minutes after the slot with no check-in.</summary>
    [HttpPost("/appointments/{id:int}/no-show")]
    [Authorize(Roles = ReceptionRoles)]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> NoShow(int id, CancellationToken ct) =>
        Ok(await _attendance.MarkNoShowAsync(id, ct));

    /// <summary>Clinician: who is in consultation and who is waiting.</summary>
    [HttpGet("/doctors/{doctorId:int}/consultation-room")]
    [Authorize(Roles = ClinicianRoles)]
    [ProducesResponseType(typeof(ConsultationRoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoom(int doctorId, CancellationToken ct) =>
        Ok(await _attendance.GetRoomAsync(doctorId, ct));

    /// <summary>Clinician: call the next waiting patient. Returns the updated room view.</summary>
    [HttpPost("/doctors/{doctorId:int}/call-next")]
    [Authorize(Roles = ClinicianRoles)]
    [ProducesResponseType(typeof(ConsultationRoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CallNext(int doctorId, CancellationToken ct) =>
        Ok(await _attendance.CallNextAsync(doctorId, ct));

    /// <summary>Clinician: finish the consultation.</summary>
    [HttpPost("/appointments/{id:int}/complete")]
    [Authorize(Roles = ClinicianRoles)]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(int id, CancellationToken ct) =>
        Ok(await _attendance.CompleteAsync(id, ct));
}
