using Clinic.Application.DTOs.Schedules;
using Clinic.Domain.Constants;
using FluentValidation;

namespace Clinic.Application.Validators;

public class UpsertScheduleValidator : AbstractValidator<UpsertScheduleDto>
{
    public UpsertScheduleValidator()
    {
        RuleFor(x => x.Days)
            .Cascade(CascadeMode.Stop) // stop at NotNull so the rules below never see a null list
            .NotNull().WithMessage("Days is required (send an empty list to clear the schedule).")
            .Must(days => days.Count <= 7).WithMessage("A week has at most 7 days.")
            .Must(days => days.Select(d => d.DayOfWeek).Distinct().Count() == days.Count)
            .WithMessage("Each weekday can appear only once.");

        RuleForEach(x => x.Days).SetValidator(new ScheduleDayValidator());
    }
}

// Not public: it is used by UpsertScheduleValidator only, so it is not picked up by assembly scanning.
internal sealed class ScheduleDayValidator : AbstractValidator<ScheduleDayDto>
{
    public ScheduleDayValidator()
    {
        RuleFor(x => x.DayOfWeek).IsInEnum();

        RuleFor(x => x.SlotMinutes)
            .InclusiveBetween(ClinicDefaults.MinSlotMinutes, ClinicDefaults.MaxSlotMinutes)
            .WithMessage($"Slot length must be between {ClinicDefaults.MinSlotMinutes} and {ClinicDefaults.MaxSlotMinutes} minutes.");

        RuleFor(x => x.CloseTime)
            .Must((day, close) => close > day.OpenTime)
            .WithMessage("Closing time must be after opening time.");

        RuleFor(x => x)
            .Must(day => (day.CloseTime.ToTimeSpan() - day.OpenTime.ToTimeSpan()).TotalMinutes >= day.SlotMinutes)
            .When(day => day.CloseTime > day.OpenTime)
            .WithMessage("The working hours must be long enough for at least one slot.");
    }
}
