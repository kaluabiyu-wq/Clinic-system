using Clinic.Api.Auth;
using Clinic.Application.DTOs.Users;
using Clinic.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("users")]
[Authorize(Policy = Policies.AdministratorOnly)]
public class UsersController : ControllerBase
{
    private readonly UserService _service;
    public UsersController(UserService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StaffUserDto>>> List(CancellationToken ct) =>
        Ok(await _service.ListAsync(ct));

    [HttpPost]
    public async Task<ActionResult<StaffUserDto>> Create(CreateStaffUserDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(List), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StaffUserDto>> Update(string id, UpdateStaffUserDto dto, CancellationToken ct) =>
        Ok(await _service.UpdateAsync(id, dto, ct));
}
