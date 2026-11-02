using Clinic.Application.DTOs.Reports;
using FluentValidation;

namespace Clinic.Application.Validators;

public class ReportFilterValidator : AbstractValidator<ReportFilterDto>
{
    /// <summary>Longest range one report can cover.</summary>
    public const int MaxRangeDays = 366;

    public ReportFilterValidator()
    {
        RuleFor(x => x.DoctorId).GreaterThan(0).When(x => x.DoctorId.HasValue);

        RuleFor(x => x.To)
            .Cascade(CascadeMode.Stop)
            .Must((filter, to) => to!.Value >= filter.From!.Value)
            .WithMessage("The end date cannot be before the start date.")
            .Must((filter, to) => to!.Value.DayNumber - filter.From!.Value.DayNumber < MaxRangeDays)
            .WithMessage($"A report can cover at most {MaxRangeDays} days.")
            .When(x => x.From.HasValue && x.To.HasValue);
    }
}
