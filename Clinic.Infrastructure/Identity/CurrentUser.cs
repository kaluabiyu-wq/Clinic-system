using System.Security.Claims;
using Clinic.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Clinic.Infrastructure.Identity;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;
    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);
}
