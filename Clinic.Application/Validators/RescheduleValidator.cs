using Clinic.Application.DTOs.Appointments;
using Clinic.Domain.Constants;
using Clinic.Domain.Rules;
using FluentValidation;

namespace Clinic.Application.Validators;

public class RescheduleValidator : AbstractValidator<RescheduleDto>
{
    public RescheduleValidator()
    {
        RuleFor(x => x.NewDate)
            .Cascade(CascadeMode.Stop)
            .Must(d => d >= ClinicTime.Today(DateTime.UtcNow))
            .WithMessage("New date cannot be in the past.")
            .Must(d => d <= ClinicTime.Today(DateTime.UtcNow).AddDays(ClinicDefaults.MaxBookingDaysAhead))
            .WithMessage($"Appointments can be booked up to {ClinicDefaults.MaxBookingDaysAhead} days ahead.");

        RuleFor(x => x.PhoneNumber)
            .Must(p => PhoneNumberNormalizer.TryNormalize(p, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Enter a valid Ethiopian mobile number, e.g. 0911223344.");
    }
}
