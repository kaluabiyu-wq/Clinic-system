using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Patients;
using Clinic.Application.Mapping;
using Clinic.Domain.Common;
using Clinic.Domain.Entities;
using Clinic.Domain.Rules;

namespace Clinic.Application.Services;

public class PatientService : IPatientService
{
    private const int MinSearchLength = 2;
    private const int MaxSearchResults = 20;

    private readonly IPatientRepository _patients;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;

    public PatientService(IPatientRepository patients, IUnitOfWork uow, IClock clock)
    {
        _patients = patients;
        _uow = uow;
        _clock = clock;
    }

    public async Task<CreatePatientResultDto> CreateAsync(CreatePatientDto dto, CancellationToken ct = default)
    {
        var fullName = NormalizeName(dto.FullName);
        var phone = PhoneNumberNormalizer.Normalize(dto.PhoneNumber);

        // FR2: warn about possible duplicates unless the receptionist confirmed.
        if (!dto.ConfirmNotDuplicate)
        {
            var duplicates = await _patients.FindPossibleDuplicatesAsync(phone, fullName, dto.DateOfBirth, ct);
            if (duplicates.Count > 0)
            {
                return new CreatePatientResultDto(
                    Created: false,
                    Patient: null,
                    PossibleDuplicates: duplicates.Select(d => d.ToSearchResult()).ToList());
            }
        }

        var patient = new Patient
        {
            FullName = fullName,
            PhoneNumber = phone,
            DateOfBirth = dto.DateOfBirth,
            Sex = dto.Sex,
            Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
            CreatedAt = _clock.UtcNow
        };

        await _patients.AddAsync(patient, ct);
        await _uow.SaveChangesAsync(ct);

        return new CreatePatientResultDto(true, patient.ToDto(), Array.Empty<PatientSearchResultDto>());
    }

    public async Task<IReadOnlyList<PatientSearchResultDto>> SearchAsync(string? query, CancellationToken ct = default)
    {
        var q = query?.Trim();
        if (string.IsNullOrEmpty(q) || q.Length < MinSearchLength)
            throw new DomainException($"Search text must be at least {MinSearchLength} characters.");

        var results = await _patients.SearchAsync(q, MaxSearchResults, ct);
        return results.Select(p => p.ToSearchResult()).ToList();
    }

    public async Task<PatientDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var patient = await _patients.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Patient {id} was not found.");

        return patient.ToDto();
    }

    private static string NormalizeName(string name) =>
        string.Join(' ', name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
