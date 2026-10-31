using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Doctors;
using Clinic.Application.Mapping;
using Clinic.Domain.Common;
using Clinic.Domain.Constants;
using Clinic.Domain.Entities;

namespace Clinic.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctors;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;

    public DoctorService(IDoctorRepository doctors, IUnitOfWork uow, IClock clock)
    {
        _doctors = doctors;
        _uow = uow;
        _clock = clock;
    }

    public async Task<IReadOnlyList<DoctorSummaryDto>> GetActiveAsync(CancellationToken ct = default)
    {
        var doctors = await _doctors.GetAllAsync(includeInactive: false, ct);
        return doctors.Select(d => d.ToSummary()).ToList();
    }

    public async Task<IReadOnlyList<DoctorDto>> GetAllAsync(CancellationToken ct = default)
    {
        var doctors = await _doctors.GetAllAsync(includeInactive: true, ct);
        return doctors.Select(d => d.ToDto()).ToList();
    }

    public async Task<DoctorSummaryDto> GetSummaryByIdAsync(int id, CancellationToken ct = default)
    {
        var doctor = await _doctors.GetByIdAsync(id, ct);

        // Inactive doctors are treated as missing on the public side.
        if (doctor is null || !doctor.IsActive)
            throw new NotFoundException($"Doctor {id} was not found.");

        return doctor.ToSummary();
    }

    public async Task<DoctorDto> CreateAsync(CreateDoctorDto dto, CancellationToken ct = default)
    {
        var letter = dto.QueueLetter.Trim().ToUpperInvariant();

        if (await _doctors.QueueLetterExistsAsync(letter, ct))
            throw new ConflictException($"Queue letter '{letter}' is already used by another doctor.");

        var doctor = new Doctor
        {
            FullName = NormalizeName(dto.FullName),
            Specialty = dto.Specialty.Trim(),
            QueueLetter = letter,
            RoomNumber = NullIfBlank(dto.RoomNumber),
            AvgConsultMinutes = dto.AvgConsultMinutes ?? ClinicDefaults.AvgConsultMinutes,
            IsActive = true,
            CreatedAt = _clock.UtcNow
        };

        await _doctors.AddAsync(doctor, ct);
        await _uow.SaveChangesAsync(ct);

        return doctor.ToDto();
    }

    public async Task<DoctorDto> UpdateAsync(int id, UpdateDoctorDto dto, CancellationToken ct = default)
    {
        var doctor = await _doctors.GetByIdForUpdateAsync(id, ct)
            ?? throw new NotFoundException($"Doctor {id} was not found.");

        // TODO when Appointment exists: refuse to deactivate a doctor who still has future
        // active appointments, so booked patients are not silently left without a doctor.

        doctor.FullName = NormalizeName(dto.FullName);
        doctor.Specialty = dto.Specialty.Trim();
        doctor.RoomNumber = NullIfBlank(dto.RoomNumber);
        doctor.AvgConsultMinutes = dto.AvgConsultMinutes;
        doctor.IsActive = dto.IsActive;
        doctor.UpdatedAt = _clock.UtcNow;

        await _uow.SaveChangesAsync(ct);

        return doctor.ToDto();
    }

    private static string NormalizeName(string name) =>
        string.Join(' ', name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
