using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Queue;
using Clinic.Domain.Common;
using Clinic.Domain.Entities;
using Clinic.Domain.Enums;
using Clinic.Domain.Rules;

namespace Clinic.Application.Services;

public class QueueService : IQueueService
{
    private const int MaxQueueNumberAttempts = 5;
    private const int BoardNextUpCount = 5;
    private const string NotFoundMessage = "Appointment not found.";

    private readonly IAppointmentRepository _appointments;
    private readonly IQueueRepository _queue;
    private readonly IDoctorRepository _doctors;
    private readonly IScheduleRepository _schedules;
    private readonly IPatientRepository _patients;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly ICurrentUser _user;

    public QueueService(
        IAppointmentRepository appointments,
        IQueueRepository queue,
        IDoctorRepository doctors,
        IScheduleRepository schedules,
        IPatientRepository patients,
        IUnitOfWork uow,
        IClock clock,
        ICurrentUser user)
    {
        _appointments = appointments;
        _queue = queue;
        _doctors = doctors;
        _schedules = schedules;
        _patients = patients;
        _uow = uow;
        _clock = clock;
        _user = user;
    }

    // ---------- Check-in (FR9) ----------

    public async Task<CheckInResultDto> CheckInAsync(int appointmentId, CancellationToken ct = default)
    {
        var appointment = await _appointments.GetByIdForUpdateAsync(appointmentId, ct)
            ?? throw new NotFoundException(NotFoundMessage);

        var doctor = appointment.Doctor ?? throw new NotFoundException(NotFoundMessage);
        var utcNow = _clock.UtcNow;

        if (appointment.Status is AppointmentStatus.Waiting or AppointmentStatus.Serving)
            throw new DomainException($"This patient is already checked in (queue number {appointment.QueueNo}).");

        StatusTransitions.EnsureCanTransition(appointment.Status, AppointmentStatus.Waiting);

        if (appointment.Date != ClinicTime.Today(utcNow))
            throw new DomainException("Only today's appointments can be checked in.");

        var from = appointment.Status;
        appointment.Status = AppointmentStatus.Waiting;
        appointment.CheckInAt = utcNow;
        appointment.UpdatedAt = utcNow;

        var auditEvent = NewEvent(from, AppointmentStatus.Waiting, utcNow);
        appointment.Events.Add(auditEvent);

        await SaveWithQueueNumberAsync(appointment, doctor, auditEvent, "Checked in", ct);

        return await BuildCheckInResultAsync(appointment, doctor, ct);
    }

    // ---------- Walk-in (FR12) ----------

    public async Task<CheckInResultDto> WalkInAsync(WalkInDto dto, CancellationToken ct = default)
    {
        var doctor = await _doctors.GetByIdAsync(dto.DoctorId, ct);
        if (doctor is null || !doctor.IsActive)
            throw new NotFoundException($"Doctor {dto.DoctorId} was not found.");

        var utcNow = _clock.UtcNow;
        var clinicNow = ClinicTime.ToLocal(utcNow);
        var today = DateOnly.FromDateTime(clinicNow);

        if (await _schedules.ClosureExistsAsync(doctor.Id, today, ct))
            throw new DomainException("The doctor is closed today.");

        // A walk-in has no booked slot: the arrival time is stored instead, and walk-ins
        // are left out of the double-booking index.
        var appointment = new Appointment
        {
            DoctorId = doctor.Id,
            Date = today,
            SlotTime = TimeOnly.FromDateTime(clinicNow),
            Status = AppointmentStatus.Waiting,
            IsWalkIn = true,
            CheckInAt = utcNow,
            CreatedAt = utcNow
        };

        await ResolvePatientAsync(appointment, dto, utcNow, ct);

        var auditEvent = NewEvent(null, AppointmentStatus.Waiting, utcNow);
        appointment.Events.Add(auditEvent);

        await _appointments.AddAsync(appointment, ct);
        await SaveWithQueueNumberAsync(appointment, doctor, auditEvent, "Walk-in registered", ct);

        return await BuildCheckInResultAsync(appointment, doctor, ct);
    }

    // ---------- Public board (FR10) ----------

    public async Task<QueueBoardDto> GetBoardAsync(CancellationToken ct = default)
    {
        var utcNow = _clock.UtcNow;
        var today = ClinicTime.Today(utcNow);

        var doctors = await _doctors.GetAllAsync(includeInactive: false, ct);
        var active = await _queue.GetActiveQueueAsync(today, ct);

        var cards = doctors.Select(d =>
        {
            var forDoctor = active.Where(a => a.DoctorId == d.Id).ToList();

            var serving = forDoctor.FirstOrDefault(a => a.Status == AppointmentStatus.Serving);

            var waiting = forDoctor
                .Where(a => a.Status == AppointmentStatus.Waiting)
                .OrderBy(a => a.CheckInAt)
                .ThenBy(a => a.Id)
                .ToList();

            return new QueueBoardCardDto(
                d.Id,
                d.FullName,
                d.RoomNumber,
                d.QueueLetter,
                serving?.QueueNo,
                waiting.Take(BoardNextUpCount).Select(a => a.QueueNo ?? string.Empty).ToList(),
                waiting.Count);
        }).ToList();

        return new QueueBoardDto(utcNow, cards);
    }

    // ---------- Track my queue (FR11) ----------

    public async Task<TrackQueueDto> TrackAsync(string reference, CancellationToken ct = default)
    {
        if (!ReferenceGenerator.TryParseId(reference, out var id))
            throw new NotFoundException(NotFoundMessage);

        var appointment = await _appointments.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(NotFoundMessage);

        var doctor = appointment.Doctor ?? throw new NotFoundException(NotFoundMessage);
        var utcNow = _clock.UtcNow;

        string? nowServing = null;
        int? peopleAhead = null;
        int? estimate = null;

        // Queue information only exists for today.
        if (appointment.Date == ClinicTime.Today(utcNow))
        {
            if (appointment.Status == AppointmentStatus.Waiting)
            {
                var wait = await ComputeWaitAsync(doctor, appointment.Id, appointment.Date, utcNow, ct);
                nowServing = wait.NowServing;
                peopleAhead = wait.PeopleAhead;
                estimate = wait.EstimatedMinutes;
            }
            else
            {
                var serving = await _queue.GetServingAsync(doctor.Id, appointment.Date, includePatient: false, ct);
                nowServing = serving?.QueueNo;

                if (appointment.Status == AppointmentStatus.Serving)
                {
                    peopleAhead = 0;
                    estimate = 0;
                }
            }
        }

        return new TrackQueueDto(
            appointment.Reference,
            appointment.Status,
            appointment.Date,
            appointment.SlotTime,
            doctor.FullName,
            doctor.RoomNumber,
            appointment.QueueNo,
            nowServing,
            peopleAhead,
            estimate);
    }

    // ---------- Helpers ----------

    private sealed record WaitInfo(int PeopleAhead, int EstimatedMinutes, string? NowServing);

    private async Task<WaitInfo> ComputeWaitAsync(
        Doctor doctor, int appointmentId, DateOnly date, DateTime utcNow, CancellationToken ct)
    {
        var waiting = await _queue.GetWaitingAsync(doctor.Id, date, includePatient: false, ct);
        var serving = await _queue.GetServingAsync(doctor.Id, date, includePatient: false, ct);

        // Waiting patients who arrived before this one.
        var peopleAhead = waiting.TakeWhile(w => w.Id != appointmentId).Count();

        int? minutesSpent = null;
        if (serving?.StartAt is not null)
            minutesSpent = (int)Math.Max(0, (utcNow - serving.StartAt.Value).TotalMinutes);

        var estimate = WaitEstimator.EstimateMinutes(doctor.AvgConsultMinutes, minutesSpent, peopleAhead);

        return new WaitInfo(peopleAhead, estimate, serving?.QueueNo);
    }

    private async Task<CheckInResultDto> BuildCheckInResultAsync(Appointment appointment, Doctor doctor, CancellationToken ct)
    {
        var wait = await ComputeWaitAsync(doctor, appointment.Id, appointment.Date, _clock.UtcNow, ct);

        return new CheckInResultDto(
            appointment.Id,
            appointment.Reference,
            appointment.QueueNo!,
            doctor.FullName,
            doctor.RoomNumber,
            wait.PeopleAhead,
            wait.EstimatedMinutes,
            appointment.CheckInAt!.Value);
    }

    /// <summary>
    /// Gives the appointment the next queue number for its doctor and day, and saves it together with
    /// the status change in one transaction. The unique index on (doctor, date, queue number) is the real
    /// guard: if two patients are checked in at the same moment and read the same count, the database
    /// rejects the second number, and this reads the count again and retries.
    /// </summary>
    private async Task SaveWithQueueNumberAsync(
        Appointment appointment, Doctor doctor, AppointmentEvent auditEvent, string action, CancellationToken ct)
    {
        for (var attempt = 1; ; attempt++)
        {
            var issued = await _queue.CountQueueNumbersIssuedAsync(appointment.DoctorId, appointment.Date, ct);

            appointment.QueueNo = QueueNumberFormatter.Format(doctor.QueueLetter, issued + 1);
            auditEvent.Note = $"{action}, queue number {appointment.QueueNo}";

            try
            {
                await _uow.SaveChangesAsync(ct);
                return;
            }
            catch (ConflictException) when (attempt < MaxQueueNumberAttempts)
            {
                // Another check-in took that number first. Read the count again and retry.
            }
        }
    }

    /// <summary>Walk-ins are registered by staff: use the PatientId, or find or create the patient from the details.</summary>
    private async Task ResolvePatientAsync(Appointment appointment, WalkInDto dto, DateTime utcNow, CancellationToken ct)
    {
        if (dto.PatientId is not null)
        {
            var patient = await _patients.GetByIdAsync(dto.PatientId.Value, ct)
                ?? throw new NotFoundException($"Patient {dto.PatientId} was not found.");

            appointment.PatientId = patient.Id;
            return;
        }

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

        appointment.Patient = new Patient
        {
            FullName = name,
            PhoneNumber = phone,
            DateOfBirth = birthDate,
            Sex = dto.Sex!.Value,
            CreatedAt = utcNow
        };
    }

    private AppointmentEvent NewEvent(AppointmentStatus? from, AppointmentStatus to, DateTime utcNow) =>
        new()
        {
            FromStatus = from,
            ToStatus = to,
            ChangedByUserId = _user.UserId,
            CreatedAt = utcNow
        };
}
