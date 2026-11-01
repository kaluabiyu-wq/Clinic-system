using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Appointments;
using Clinic.Application.DTOs.Schedules;
using Clinic.Application.Mapping;
using Clinic.Domain.Common;
using Clinic.Domain.Constants;
using Clinic.Domain.Entities;
using Clinic.Domain.Enums;
using Clinic.Domain.Rules;

namespace Clinic.Application.Services;

public class BookingService : IBookingService
{
    private const string SlotTakenMessage = "That time slot was just taken. Please choose another slot.";
    private const string NotFoundMessage = "Appointment not found.";

    private readonly IAppointmentRepository _appointments;
    private readonly IScheduleRepository _schedules;
    private readonly IDoctorRepository _doctors;
    private readonly IPatientRepository _patients;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly ICurrentUser _user;

    public BookingService(
        IAppointmentRepository appointments,
        IScheduleRepository schedules,
        IDoctorRepository doctors,
        IPatientRepository patients,
        IUnitOfWork uow,
        IClock clock,
        ICurrentUser user)
    {
        _appointments = appointments;
        _schedules = schedules;
        _doctors = doctors;
        _patients = patients;
        _uow = uow;
        _clock = clock;
        _user = user;
    }

    // ---------- Free slots ----------

    public async Task<IReadOnlyList<SlotDto>> GetFreeSlotsAsync(int doctorId, DateOnly date, CancellationToken ct = default)
    {
        await GetActiveDoctorAsync(doctorId, ct);

        var availability = await LoadAvailabilityAsync(doctorId, date, ct);
        if (availability.Day is null)
            return Array.Empty<SlotDto>();

        var length = TimeSpan.FromMinutes(availability.Day.SlotMinutes);
        var clinicNow = ClinicTime.ToLocal(_clock.UtcNow);

        return SlotAvailabilityChecker
            .GetFreeSlots(availability.Day, availability.Closed, date, availability.Taken, clinicNow)
            .Select(start => new SlotDto(start, TimeOnly.FromTimeSpan(start.ToTimeSpan() + length)))
            .ToList();
    }

    // ---------- Book ----------

    public async Task<AppointmentDto> BookAsync(BookAppointmentDto dto, CancellationToken ct = default)
    {
        var doctor = await GetActiveDoctorAsync(dto.DoctorId, ct);

        await EnsureSlotAvailableAsync(doctor.Id, dto.Date, dto.SlotTime, ct);

        var utcNow = _clock.UtcNow;

        var appointment = new Appointment
        {
            DoctorId = doctor.Id,
            Date = dto.Date,
            SlotTime = dto.SlotTime,
            Status = AppointmentStatus.Booked,
            IsWalkIn = false,
            CreatedAt = utcNow
        };

        if (dto.PatientId is not null)
        {
            if (!IsStaff())
                throw new DomainException(
                    "Booking for a registered patient is for staff only. Send your name, phone number, date of birth and sex instead.");

            var patient = await _patients.GetByIdAsync(dto.PatientId.Value, ct)
                ?? throw new NotFoundException($"Patient {dto.PatientId} was not found.");

            appointment.PatientId = patient.Id;
        }
        else
        {
            await AttachSelfServicePatientAsync(appointment, dto, utcNow, ct);
        }

        appointment.Events.Add(NewEvent(null, AppointmentStatus.Booked, "Booked", utcNow));

        await _appointments.AddAsync(appointment, ct);
        await SaveBookingAsync(ct);

        // Reload so the response has the doctor and patient names.
        var saved = await _appointments.GetByIdAsync(appointment.Id, ct)
            ?? throw new NotFoundException(NotFoundMessage);

        return saved.ToDto();
    }

    // ---------- Look up, reschedule, cancel ----------

    public async Task<AppointmentDto> GetByReferenceAsync(string reference, string? phoneNumber, CancellationToken ct = default)
    {
        var appointment = await LoadOwnedAsync(reference, phoneNumber, forUpdate: false, ct);
        return appointment.ToDto();
    }

    public async Task<AppointmentDto> RescheduleAsync(string reference, RescheduleDto dto, CancellationToken ct = default)
    {
        var appointment = await LoadOwnedAsync(reference, dto.PhoneNumber, forUpdate: true, ct);
        var utcNow = _clock.UtcNow;

        if (appointment.Status != AppointmentStatus.Booked)
            throw new DomainException("Only a booked appointment that has not been checked in can be rescheduled.");

        EnsureNotPastForPatient(appointment, utcNow);

        if (appointment.Doctor is null || !appointment.Doctor.IsActive)
            throw new DomainException("This doctor is no longer taking appointments. Please contact reception.");

        if (dto.NewDate == appointment.Date && dto.NewSlotTime == appointment.SlotTime)
            throw new DomainException("Choose a different date or time from the current appointment.");

        await EnsureSlotAvailableAsync(appointment.DoctorId, dto.NewDate, dto.NewSlotTime, ct);

        var note = $"Rescheduled from {appointment.Date:yyyy-MM-dd} {appointment.SlotTime:HH:mm} " +
                   $"to {dto.NewDate:yyyy-MM-dd} {dto.NewSlotTime:HH:mm}";

        appointment.Date = dto.NewDate;
        appointment.SlotTime = dto.NewSlotTime;
        appointment.UpdatedAt = utcNow;
        appointment.Events.Add(NewEvent(AppointmentStatus.Booked, AppointmentStatus.Booked, note, utcNow));

        await SaveBookingAsync(ct);

        return appointment.ToDto();
    }

    public async Task<AppointmentDto> CancelAsync(string reference, CancelDto dto, CancellationToken ct = default)
    {
        var appointment = await LoadOwnedAsync(reference, dto.PhoneNumber, forUpdate: true, ct);
        var utcNow = _clock.UtcNow;

        // After check-in only reception can cancel.
        if (!IsStaff() && appointment.Status != AppointmentStatus.Booked)
            throw new DomainException("Please contact reception to cancel after check-in.");

        StatusTransitions.EnsureCanTransition(appointment.Status, AppointmentStatus.Cancelled);
        EnsureNotPastForPatient(appointment, utcNow);

        var from = appointment.Status;
        var reason = string.IsNullOrWhiteSpace(dto.Reason) ? null : dto.Reason.Trim();

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = utcNow;
        appointment.Events.Add(NewEvent(from, AppointmentStatus.Cancelled, reason is null ? "Cancelled" : $"Cancelled: {reason}", utcNow));

        await _uow.SaveChangesAsync(ct);

        return appointment.ToDto();
    }

    // ---------- Staff list ----------

    public async Task<IReadOnlyList<AppointmentDto>> ListAsync(AppointmentListFilterDto filter, CancellationToken ct = default)
    {
        var date = filter.Date ?? ClinicTime.Today(_clock.UtcNow);

        var appointments = await _appointments.ListAsync(date, filter.DoctorId, filter.Status, ct);
        return appointments.Select(a => a.ToDto()).ToList();
    }

    // ---------- Helpers ----------

    private bool IsStaff() =>
        _user.IsInRole(RoleNames.Receptionist) || _user.IsInRole(RoleNames.Administrator);

    private async Task<Doctor> GetActiveDoctorAsync(int doctorId, CancellationToken ct)
    {
        var doctor = await _doctors.GetByIdAsync(doctorId, ct);

        if (doctor is null || !doctor.IsActive)
            throw new NotFoundException($"Doctor {doctorId} was not found.");

        return doctor;
    }

    private sealed record Availability(Schedule? Day, bool Closed, IReadOnlyList<TimeOnly> Taken);

    private async Task<Availability> LoadAvailabilityAsync(int doctorId, DateOnly date, CancellationToken ct)
    {
        var closed = await _schedules.ClosureExistsAsync(doctorId, date, ct);
        var day = await _schedules.GetByDoctorAndDayAsync(doctorId, date.DayOfWeek, ct);
        var taken = await _appointments.GetTakenSlotsAsync(doctorId, date, ct);

        return new Availability(day, closed, taken);
    }

    private async Task EnsureSlotAvailableAsync(int doctorId, DateOnly date, TimeOnly slot, CancellationToken ct)
    {
        var availability = await LoadAvailabilityAsync(doctorId, date, ct);
        var clinicNow = ClinicTime.ToLocal(_clock.UtcNow);

        var result = SlotAvailabilityChecker.Check(
            availability.Day, availability.Closed, date, slot, availability.Taken, clinicNow);

        switch (result)
        {
            case SlotCheckResult.Available:
                return;
            case SlotCheckResult.Taken:
                throw new ConflictException(SlotTakenMessage);
            case SlotCheckResult.DoctorClosed:
                throw new DomainException("The doctor is closed on that date.");
            case SlotCheckResult.DoctorNotWorking:
                throw new DomainException("The doctor does not work on that day.");
            case SlotCheckResult.NotASlot:
                throw new DomainException("That is not a valid appointment time for this doctor.");
            case SlotCheckResult.InPast:
                throw new DomainException("That time has already passed.");
            default:
                throw new DomainException("That slot is not available.");
        }
    }

    /// <summary>
    /// Saves a booking or reschedule. The partial unique index rejects a second active appointment
    /// for the same doctor, date and slot, and the UnitOfWork turns that into a ConflictException.
    /// </summary>
    private async Task SaveBookingAsync(CancellationToken ct)
    {
        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (ConflictException)
        {
            throw new ConflictException(SlotTakenMessage);
        }
    }

    private async Task AttachSelfServicePatientAsync(
        Appointment appointment, BookAppointmentDto dto, DateTime utcNow, CancellationToken ct)
    {
        var phone = PhoneNumberNormalizer.Normalize(dto.PhoneNumber!);
        var name = NameNormalizer.Normalize(dto.FullName!);
        var birthDate = dto.DateOfBirth!.Value;

        // The same person if phone, name (any letter case) and date of birth all match.
        var candidates = await _patients.FindPossibleDuplicatesAsync(phone, name, birthDate, ct);
        var existing = candidates.FirstOrDefault(p =>
            p.PhoneNumber == phone &&
            p.DateOfBirth == birthDate &&
            string.Equals(p.FullName, name, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            appointment.PatientId = existing.Id;
            return;
        }

        // New patient, saved in the same transaction as the appointment: if the slot
        // was just taken, nothing is saved.
        appointment.Patient = new Patient
        {
            FullName = name,
            PhoneNumber = phone,
            DateOfBirth = birthDate,
            Sex = dto.Sex!.Value,
            CreatedAt = utcNow
        };
    }

    /// <summary>
    /// Finds an appointment by reference. Patients must also give the phone number on file;
    /// a wrong reference and a wrong phone give the same error, so references cannot be probed.
    /// </summary>
    private async Task<Appointment> LoadOwnedAsync(string reference, string? phoneNumber, bool forUpdate, CancellationToken ct)
    {
        if (!ReferenceGenerator.TryParseId(reference, out var id))
            throw new NotFoundException(NotFoundMessage);

        var appointment = forUpdate
            ? await _appointments.GetByIdForUpdateAsync(id, ct)
            : await _appointments.GetByIdAsync(id, ct);

        if (appointment is null)
            throw new NotFoundException(NotFoundMessage);

        if (!IsStaff())
        {
            if (!PhoneNumberNormalizer.TryNormalize(phoneNumber, out var phone) ||
                appointment.Patient?.PhoneNumber != phone)
                throw new NotFoundException(NotFoundMessage);
        }

        return appointment;
    }

    /// <summary>Patients cannot change an appointment whose time has passed; reception can.</summary>
    private void EnsureNotPastForPatient(Appointment appointment, DateTime utcNow)
    {
        if (IsStaff())
            return;

        if (appointment.Date.ToDateTime(appointment.SlotTime) <= ClinicTime.ToLocal(utcNow))
            throw new DomainException("This appointment time has passed. Please contact reception.");
    }

    private AppointmentEvent NewEvent(AppointmentStatus? from, AppointmentStatus to, string note, DateTime utcNow) =>
        new()
        {
            FromStatus = from,
            ToStatus = to,
            Note = note,
            ChangedByUserId = _user.UserId, // null = the patient acted on their own
            CreatedAt = utcNow
        };
}
