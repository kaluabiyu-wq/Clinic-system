using Clinic.Application.DTOs.Appointments;
using Clinic.Domain.Rules;
using FluentValidation;

namespace Clinic.Application.Validators;

public class CancelValidator : AbstractValidator<CancelDto>
{
    public CancelValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(200);

        RuleFor(x => x.PhoneNumber)
            .Must(p => PhoneNumberNormalizer.TryNormalize(p, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Enter a valid Ethiopian mobile number, e.g. 0911223344.");
    }
}
