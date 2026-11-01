namespace Clinic.Application.Abstractions;

/// <summary>The caller of the current request. Anonymous callers (patients booking online) are not authenticated.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    /// <summary>Identity user id, or null for anonymous callers.</summary>
    string? UserId { get; }

    bool IsInRole(string role);
}
