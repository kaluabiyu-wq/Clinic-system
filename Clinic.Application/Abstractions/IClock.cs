namespace Clinic.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
