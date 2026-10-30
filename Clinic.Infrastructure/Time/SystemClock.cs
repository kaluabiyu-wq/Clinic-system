using Clinic.Application.Abstractions;

namespace Clinic.Infrastructure.Time;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
