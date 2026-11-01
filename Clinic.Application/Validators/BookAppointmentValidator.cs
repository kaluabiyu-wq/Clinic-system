using Clinic.Application.DTOs.Appointments;
using Clinic.Domain.Constants;
using Clinic.Domain.Rules;
using FluentValidation;

namespace Clinic.Application.Validators;

public class BookAppointmentValidator : AbstractValidator<BookAppointmentDto>
{
    public BookAppointmentValidator()
    {
        RuleFor(x => x.DoctorId).GreaterThan(0);

        RuleFor(x => x.Date)
            .Cascade(CascadeMode.Stop)
            .Must(d => d >= ClinicTime.Today(DateTime.UtcNow))
            .WithMessage("Date cannot be in the past.")
            .Must(d => d <= ClinicTime.Today(DateTime.UtcNow).AddDays(ClinicDefaults.MaxBookingDaysAhead))
            .WithMessage($"Appointments can be booked up to {ClinicDefaults.MaxBookingDaysAhead} days ahead.");

        RuleFor(x => x.PatientId).GreaterThan(0).When(x => x.PatientId.HasValue);

        // Self-service: no PatientId, so the patient's own details are required.
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
