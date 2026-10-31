using Clinic.Application.DTOs.Doctors;
using Clinic.Domain.Entities;

namespace Clinic.Application.Mapping;

public static class DoctorMapping
{
    public static DoctorDto ToDto(this Doctor d) =>
        new(d.Id, d.FullName, d.Specialty, d.QueueLetter, d.RoomNumber, d.AvgConsultMinutes, d.IsActive, d.CreatedAt);

    public static DoctorSummaryDto ToSummary(this Doctor d) =>
        new(d.Id, d.FullName, d.Specialty, d.RoomNumber);
}
