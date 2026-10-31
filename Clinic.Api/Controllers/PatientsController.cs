using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Patients;
using Clinic.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("patients")]
//[Authorize(Roles = $"{RoleNames.Receptionist},{RoleNames.Administrator}")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _service;
    private readonly IValidator<CreatePatientDto> _validator;

    public PatientsController(IPatientService service, IValidator<CreatePatientDto> validator)
    {
        _service = service;
        _validator = validator;
    }

   
    [HttpPost]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreatePatientDto dto, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await _service.CreateAsync(dto, ct);

        if (!result.Created)
        {
            return Conflict(new
            {
                message = "A possible duplicate patient already exists. Review the matches, or resubmit with confirmNotDuplicate = true.",
                possibleDuplicates = result.PossibleDuplicates
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Patient!.Id }, result.Patient);
    }

    
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PatientSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string? q, CancellationToken ct) =>
        Ok(await _service.SearchAsync(q, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) =>
        Ok(await _service.GetByIdAsync(id, ct));
}
