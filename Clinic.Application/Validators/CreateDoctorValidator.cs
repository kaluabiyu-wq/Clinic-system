using Clinic.Application.DTOs.Doctors;
using Clinic.Domain.Constants;
using FluentValidation;

namespace Clinic.Application.Validators;

public class CreateDoctorValidator : AbstractValidator<CreateDoctorDto>
{
    public CreateDoctorValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(3).WithMessage("Full name must be at least 3 characters.")
            .MaximumLength(150);

        RuleFor(x => x.Specialty)
            .NotEmpty().WithMessage("Specialty is required.")
            .MaximumLength(100);

        RuleFor(x => x.QueueLetter)
            .NotEmpty().WithMessage("Queue letter is required.")
            .Matches("^[A-Za-z]$").WithMessage("Queue letter must be a single letter A-Z.");

        RuleFor(x => x.RoomNumber).MaximumLength(20);

        RuleFor(x => x.AvgConsultMinutes)
            .InclusiveBetween(ClinicDefaults.MinConsultMinutes, ClinicDefaults.MaxConsultMinutes)
            .When(x => x.AvgConsultMinutes.HasValue)
            .WithMessage($"Average consultation must be between {ClinicDefaults.MinConsultMinutes} and {ClinicDefaults.MaxConsultMinutes} minutes.");
    }
}
