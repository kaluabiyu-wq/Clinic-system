using Clinic.Application.DTOs.Schedules;
using FluentValidation;

namespace Clinic.Application.Validators;

public class CreateClosureValidator : AbstractValidator<CreateClosureDto>
{
    public CreateClosureValidator()
    {
        RuleFor(x => x.Date)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Closure date cannot be in the past.");

        RuleFor(x => x.Reason).MaximumLength(200);
    }
}
