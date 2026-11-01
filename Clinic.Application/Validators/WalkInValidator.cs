using Clinic.Application.DTOs.Queue;
using Clinic.Domain.Rules;
using FluentValidation;

namespace Clinic.Application.Validators;

public class WalkInValidator : AbstractValidator<WalkInDto>
{
    public WalkInValidator()
    {
        RuleFor(x => x.DoctorId).GreaterThan(0);
        RuleFor(x => x.PatientId).GreaterThan(0).When(x => x.PatientId.HasValue);

        // No PatientId: reception is registering a new patient, so the details are required.
        When(x => x.PatientId is null, () =>
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MinimumLength(3).WithMessage("Full name must be at least 3 characters.")
                .MaximumLength(150);

            RuleFor(x => x.PhoneNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Phone number is required.")
                .Must(p => PhoneNumberNormalizer.TryNormalize(p, out _))
                .WithMessage("Enter a valid Ethiopian mobile number, e.g. 0911223344.");

            RuleFor(x => x.DateOfBirth)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Date of birth is required.")
                .Must(d => d!.Value <= ClinicTime.Today(DateTime.UtcNow)).WithMessage("Date of birth cannot be in the future.")
                .Must(d => d!.Value.Year >= 1900).WithMessage("Date of birth is not valid.");

            RuleFor(x => x.Sex)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Sex is required.")
                .IsInEnum();
        });
    }
}
