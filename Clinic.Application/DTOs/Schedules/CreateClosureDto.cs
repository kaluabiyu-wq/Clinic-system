namespace Clinic.Application.DTOs.Schedules;

public sealed record CreateClosureDto
{
    public required DateOnly Date { get; init; }

    public string? Reason { get; init; }
}
