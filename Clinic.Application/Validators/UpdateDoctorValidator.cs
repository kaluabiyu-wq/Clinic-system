using Clinic.Application.DTOs.Doctors;
using Clinic.Domain.Constants;
using FluentValidation;

namespace Clinic.Application.Validators;

public class UpdateDoctorValidator : AbstractValidator<UpdateDoctorDto>
{
    public UpdateDoctorValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(3).WithMessage("Full name must be at least 3 characters.")
            .MaximumLength(150);

        RuleFor(x => x.Specialty)
            .NotEmpty().WithMessage("Specialty is required.")
            .MaximumLength(100);

        RuleFor(x => x.RoomNumber).MaximumLength(20);

        RuleFor(x => x.AvgConsultMinutes)
            .InclusiveBetween(ClinicDefaults.MinConsultMinutes, ClinicDefaults.MaxConsultMinutes)
            .WithMessage($"Average consultation must be between {ClinicDefaults.MinConsultMinutes} and {ClinicDefaults.MaxConsultMinutes} minutes.");
    }
}
