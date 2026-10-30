using Clinic.Application.DTOs.Patients;
using Clinic.Domain.Entities;

namespace Clinic.Application.Mapping;

public static class PatientMapping
{
    public static PatientDto ToDto(this Patient p) =>
        new(p.Id, p.FullName, p.PhoneNumber, p.DateOfBirth, p.Sex, p.Address, p.CreatedAt);

    public static PatientSearchResultDto ToSearchResult(this Patient p) =>
        new(p.Id, p.FullName, p.PhoneNumber, p.DateOfBirth);
}
