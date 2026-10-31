using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Doctors;
using Clinic.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("doctors")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _service;
    private readonly IValidator<CreateDoctorDto> _createValidator;
    private readonly IValidator<UpdateDoctorDto> _updateValidator;

    public DoctorsController(
        IDoctorService service,
        IValidator<CreateDoctorDto> createValidator,
        IValidator<UpdateDoctorDto> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Public: active doctors for the booking page.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<DoctorSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive(CancellationToken ct) =>
        Ok(await _service.GetActiveAsync(ct));

    /// <summary>Public: one active doctor.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DoctorSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) =>
        Ok(await _service.GetSummaryByIdAsync(id, ct));

    /// <summary>Administrator: all doctors including inactive, with full details.</summary>
    [HttpGet("manage")]
    [Authorize(Roles = RoleNames.Administrator)]
    [ProducesResponseType(typeof(IReadOnlyList<DoctorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await _service.GetAllAsync(ct));

    /// <summary>Administrator: add a doctor.</summary>
    [HttpPost]
    //[Authorize(Roles = RoleNames.Administrator)]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateDoctorDto dto, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var doctor = await _service.CreateAsync(dto, ct);

        return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, doctor);
    }

    /// <summary>Administrator: edit a doctor, including activating or deactivating.</summary>
    [HttpPut("{id:int}")]
    // [Authorize(Roles = RoleNames.Administrator)]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDoctorDto dto, CancellationToken ct)
    {
        var validation = await _updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        return Ok(await _service.UpdateAsync(id, dto, ct));
    }
}
