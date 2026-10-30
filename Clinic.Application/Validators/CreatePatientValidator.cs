using Clinic.Application.DTOs.Patients;
using Clinic.Domain.Rules;
using FluentValidation;

namespace Clinic.Application.Validators;

public class CreatePatientValidator : AbstractValidator<CreatePatientDto>
{
    public CreatePatientValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(3).WithMessage("Full name must be at least 3 characters.")
            .MaximumLength(150);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Must(p => PhoneNumberNormalizer.TryNormalize(p, out _))
            .WithMessage("Enter a valid Ethiopian mobile number, e.g. 0911223344.");

        RuleFor(x => x.DateOfBirth)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth cannot be in the future.")
            .Must(d => d.Year >= 1900)
            .WithMessage("Date of birth is not valid.");

        RuleFor(x => x.Sex).IsInEnum();

        RuleFor(x => x.Address).MaximumLength(250);
    }
}
