using Clinic.Application.DTOs.Users;
using Clinic.Domain.Constants;
using FluentValidation;

namespace Clinic.Application.Validators;

internal static class PasswordRules
{
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.MinimumLength(10).WithMessage("Password must be at least 10 characters.")
            .Matches("[A-Z]").WithMessage("Password needs an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password needs a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password needs a digit.");
}

public sealed class CreateStaffUserValidator : AbstractValidator<CreateStaffUserDto>
{
    public CreateStaffUserValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().StrongPassword();
        RuleFor(x => x.Role).Must(r => RoleNames.All.Contains(r))
            .WithMessage($"Role must be one of: {string.Join(", ", RoleNames.All)}.");
        RuleFor(x => x.DoctorId).NotNull().When(x => x.Role == RoleNames.Clinician)
            .WithMessage("A clinician account must be linked to a doctor.");
    }
}

public sealed class UpdateStaffUserValidator : AbstractValidator<UpdateStaffUserDto>
{
    public UpdateStaffUserValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Role).Must(r => RoleNames.All.Contains(r))
            .WithMessage($"Role must be one of: {string.Join(", ", RoleNames.All)}.");
        RuleFor(x => x.DoctorId).NotNull().When(x => x.Role == RoleNames.Clinician)
            .WithMessage("A clinician account must be linked to a doctor.");
        RuleFor(x => x.NewPassword!).StrongPassword().When(x => !string.IsNullOrEmpty(x.NewPassword));
    }
}
