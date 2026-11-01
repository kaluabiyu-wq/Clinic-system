using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Appointments;
using Clinic.Application.DTOs.Queue;
using Clinic.Application.Mapping;
using Clinic.Domain.Common;
using Clinic.Domain.Constants;
using Clinic.Domain.Entities;
using Clinic.Domain.Enums;
using Clinic.Domain.Rules;

namespace Clinic.Application.Services;

public class AttendanceService : IAttendanceService
{
    private const string NotFoundMessage = "Appointment not found.";

    private readonly IAppointmentRepository _appointments;
    private readonly IQueueRepository _queue;
    private readonly IDoctorRepository _doctors;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly ICurrentUser _user;

    public AttendanceService(
        IAppointmentRepository appointments,
        IQueueRepository queue,
        IDoctorRepository doctors,
        IUnitOfWork uow,
        IClock clock,
        ICurrentUser user)
    {
        _appointments = appointments;
        _queue = queue;
        _doctors = doctors;
        _uow = uow;
        _clock = clock;
        _user = user;
    }

    // ---------- Consultation room ----------

    public async Task<ConsultationRoomDto> GetRoomAsync(int doctorId, CancellationToken ct = default)
    {
        var doctor = await GetDoctorAsync(doctorId, ct);
        EnsureCanActFor(doctor);

        var utcNow = _clock.UtcNow;
        var today = ClinicTime.Today(utcNow);

        var serving = await _queue.GetServingAsync(doctor.Id, today, includePatient: true, ct);
        var waiting = await _queue.GetWaitingAsync(doctor.Id, today, includePatient: true, ct);

        return new ConsultationRoomDto(
            doctor.Id,
            doctor.FullName,
            doctor.RoomNumber,
            serving?.ToQueueEntry(utcNow),
            waiting.Select(w => w.ToQueueEntry(utcNow)).ToList());
    }

    // ---------- Call next (FR13) ----------

    public async Task<ConsultationRoomDto> CallNextAsync(int doctorId, CancellationToken ct = default)
    {
        var doctor = await GetDoctorAsync(doctorId, ct);
        EnsureCanActFor(doctor);

        var utcNow = _clock.UtcNow;
        var today = ClinicTime.Today(utcNow);

        if (await _queue.GetServingAsync(doctor.Id, today, includePatient: false, ct) is not null)
            throw new ConflictException("Finish the current consultation before calling the next patient.");

        var next = await _queue.GetNextWaitingForUpdateAsync(doctor.Id, today, ct)
            ?? throw new NotFoundException("No patients are waiting.");

        StatusTransitions.EnsureCanTransition(next.Status, AppointmentStatus.Serving);

        next.Status = AppointmentStatus.Serving;
        next.StartAt = utcNow;
        next.UpdatedAt = utcNow;
        next.Events.Add(NewEvent(AppointmentStatus.Waiting, AppointmentStatus.Serving, $"Called, queue number {next.QueueNo}", utcNow));

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (ConflictException)
        {
            // The database allows only one patient in consultation per doctor and day.
            throw new ConflictException("Another patient is already in consultation. Refresh and try again.");
        }

        return await GetRoomAsync(doctor.Id, ct);
    }

    // ---------- Complete (FR13) ----------

    public async Task<AppointmentDto> CompleteAsync(int appointmentId, CancellationToken ct = default)
    {
        var appointment = await _appointments.GetByIdForUpdateAsync(appointmentId, ct)
            ?? throw new NotFoundException(NotFoundMessage);

        EnsureCanActFor(appointment.Doctor ?? throw new NotFoundException(NotFoundMessage));

        StatusTransitions.EnsureCanTransition(appointment.Status, AppointmentStatus.Completed);

        var utcNow = _clock.UtcNow;
        var from = appointment.Status;

        appointment.Status = AppointmentStatus.Completed;
        appointment.EndAt = utcNow;
        appointment.UpdatedAt = utcNow;
        appointment.Events.Add(NewEvent(from, AppointmentStatus.Completed, "Consultation completed", utcNow));

        await _uow.SaveChangesAsync(ct);

        return appointment.ToDto();
    }

    // ---------- No-show (FR14) ----------

    public async Task<AppointmentDto> MarkNoShowAsync(int appointmentId, CancellationToken ct = default)
    {
        var appointment = await _appointments.GetByIdForUpdateAsync(appointmentId, ct)
            ?? throw new NotFoundException(NotFoundMessage);

        var utcNow = _clock.UtcNow;

        NoShowRule.EnsureCanMarkNoShow(appointment, ClinicTime.ToLocal(utcNow));
        StatusTransitions.EnsureCanTransition(appointment.Status, AppointmentStatus.NoShow);

        var from = appointment.Status;

        appointment.Status = AppointmentStatus.NoShow;
        appointment.UpdatedAt = utcNow;
        appointment.Events.Add(NewEvent(from, AppointmentStatus.NoShow, "Marked as no-show", utcNow));

        await _uow.SaveChangesAsync(ct);

        return appointment.ToDto();
    }

    // ---------- Helpers ----------

    private async Task<Doctor> GetDoctorAsync(int doctorId, CancellationToken ct) =>
        await _doctors.GetByIdAsync(doctorId, ct)
            ?? throw new NotFoundException($"Doctor {doctorId} was not found.");

    /// <summary>
    /// A clinician may only manage their own queue (Doctor.UserId must be their account).
    /// Administrators may act for any doctor. Role checks themselves are done by [Authorize] on the controller.
    /// </summary>
    private void EnsureCanActFor(Doctor doctor)
    {
        if (_user.IsInRole(RoleNames.Administrator))
            return;

        if (_user.IsInRole(RoleNames.Clinician) && (doctor.UserId is null || doctor.UserId != _user.UserId))
            throw new ForbiddenException("You can only manage your own queue.");
    }

    private AppointmentEvent NewEvent(AppointmentStatus from, AppointmentStatus to, string note, DateTime utcNow) =>
        new()
        {
            FromStatus = from,
            ToStatus = to,
            Note = note,
            ChangedByUserId = _user.UserId,
            CreatedAt = utcNow
        };
}
