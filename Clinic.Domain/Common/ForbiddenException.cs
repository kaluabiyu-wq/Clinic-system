namespace Clinic.Domain.Common;

/// <summary>The caller is signed in but not allowed to do this (for example a clinician on another doctor's queue). Map to HTTP 403.</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
