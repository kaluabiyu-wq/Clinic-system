namespace Clinic.Application.DTOs.Schedules;

public sealed record ClosureDto(
    int Id,
    DateOnly Date,
    string? Reason);
