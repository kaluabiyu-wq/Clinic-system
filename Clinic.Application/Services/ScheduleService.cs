using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Schedules;
using Clinic.Application.Mapping;
using Clinic.Domain.Common;
using Clinic.Domain.Entities;
using Clinic.Domain.Rules;

namespace Clinic.Application.Services;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _schedules;
    private readonly IDoctorRepository _doctors;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;

    public ScheduleService(
        IScheduleRepository schedules,
        IDoctorRepository doctors,
        IUnitOfWork uow,
        IClock clock)
    {
        _schedules = schedules;
        _doctors = doctors;
        _uow = uow;
        _clock = clock;
    }

    public async Task<ScheduleDto> GetScheduleAsync(int doctorId, CancellationToken ct = default)
    {
        await EnsureDoctorExistsAsync(doctorId, ct);

        var days = await _schedules.GetByDoctorAsync(doctorId, ct);
        return days.ToScheduleDto(doctorId);
    }

    public async Task<ScheduleDto> UpsertScheduleAsync(int doctorId, UpsertScheduleDto dto, CancellationToken ct = default)
    {
        await EnsureDoctorExistsAsync(doctorId, ct);

        var existing = await _schedules.GetByDoctorForUpdateAsync(doctorId, ct);
        var incoming = dto.Days.ToDictionary(d => d.DayOfWeek);

        // Weekdays that are no longer sent become days off.
        foreach (var removed in existing.Where(e => !incoming.ContainsKey(e.DayOfWeek)))
            _schedules.Remove(removed);

        // Update in place (rather than delete and re-insert) so the unique (doctor, weekday) index is never violated.
        foreach (var day in dto.Days)
        {
            var current = existing.FirstOrDefault(e => e.DayOfWeek == day.DayOfWeek);

            if (current is null)
            {
                await _schedules.AddAsync(new Schedule
                {
                    DoctorId = doctorId,
                    DayOfWeek = day.DayOfWeek,
                    OpenTime = day.OpenTime,
                    CloseTime = day.CloseTime,
                    SlotMinutes = day.SlotMinutes,
                    CreatedAt = _clock.UtcNow
                }, ct);
            }
            else
            {
                current.OpenTime = day.OpenTime;
                current.CloseTime = day.CloseTime;
                current.SlotMinutes = day.SlotMinutes;
                current.UpdatedAt = _clock.UtcNow;
            }
        }

        // TODO when Appointment exists: warn or refuse if the new hours leave booked appointments outside the schedule.

        await _uow.SaveChangesAsync(ct);

        var saved = await _schedules.GetByDoctorAsync(doctorId, ct);
        return saved.ToScheduleDto(doctorId);
    }

    public async Task<IReadOnlyList<ClosureDto>> GetClosuresAsync(int doctorId, CancellationToken ct = default)
    {
        await EnsureDoctorExistsAsync(doctorId, ct);

        var today = DateOnly.FromDateTime(_clock.UtcNow);
        var closures = await _schedules.GetClosuresFromAsync(doctorId, today, ct);
        return closures.Select(c => c.ToDto()).ToList();
    }

    public async Task<ClosureDto> CreateClosureAsync(int doctorId, CreateClosureDto dto, CancellationToken ct = default)
    {
        await EnsureDoctorExistsAsync(doctorId, ct);

        if (await _schedules.ClosureExistsAsync(doctorId, dto.Date, ct))
            throw new ConflictException($"The doctor is already closed on {dto.Date:yyyy-MM-dd}.");

        // TODO when Appointment exists: refuse (or require confirmation) if that day already has booked appointments.

        var closure = new ScheduleException
        {
            DoctorId = doctorId,
            Date = dto.Date,
            Reason = string.IsNullOrWhiteSpace(dto.Reason) ? null : dto.Reason.Trim(),
            CreatedAt = _clock.UtcNow
        };

        await _schedules.AddClosureAsync(closure, ct);
        await _uow.SaveChangesAsync(ct);

        return closure.ToDto();
    }

    public async Task DeleteClosureAsync(int doctorId, int closureId, CancellationToken ct = default)
    {
        // Looking up by both ids means one doctor's closure cannot be deleted through another doctor's URL.
        var closure = await _schedules.GetClosureAsync(doctorId, closureId, ct)
            ?? throw new NotFoundException($"Closure {closureId} was not found for doctor {doctorId}.");

        _schedules.RemoveClosure(closure);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SlotDto>> PreviewSlotsAsync(int doctorId, DateOnly date, CancellationToken ct = default)
    {
        await EnsureDoctorExistsAsync(doctorId, ct);

        if (await _schedules.ClosureExistsAsync(doctorId, date, ct))
            return Array.Empty<SlotDto>();

        var day = await _schedules.GetByDoctorAndDayAsync(doctorId, date.DayOfWeek, ct);
        if (day is null)
            return Array.Empty<SlotDto>();

        var length = TimeSpan.FromMinutes(day.SlotMinutes);

        return SlotGenerator.Generate(day.OpenTime, day.CloseTime, day.SlotMinutes)
            .Select(start => new SlotDto(start, TimeOnly.FromTimeSpan(start.ToTimeSpan() + length)))
            .ToList();
    }

    private async Task EnsureDoctorExistsAsync(int doctorId, CancellationToken ct)
    {
        if (await _doctors.GetByIdAsync(doctorId, ct) is null)
            throw new NotFoundException($"Doctor {doctorId} was not found.");
    }
}
