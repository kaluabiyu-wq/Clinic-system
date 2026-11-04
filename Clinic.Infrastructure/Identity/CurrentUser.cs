using System.Security.Claims;
using Clinic.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Clinic.Api.Auth;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public string UserId =>
        Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    public bool IsInRole(string role) =>
        Principal?.IsInRole(role) ?? false;
}