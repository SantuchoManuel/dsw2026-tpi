using System;

namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentModel
{
    public record Request(Guid DoctorId, Guid AvailabilitySlotId, PatientDto Patient, string Reason);
    public record PatientDto(int Dni);
    public record Response(Guid Id, DateTime Date, string DoctorName, string Reason);
    public record ByDateResponse(string Specialty, string Doctor, string AvailableTime, string? PatientName = null, string? Dni = null);
    public record SearchResponse(Guid AppointmentsId, DateTime Date, string AppointmentsStatus, PatientSearchDto Patient, DoctorSearchDto Doctor);
    public record PatientSearchDto(int Dni, string FullName);
    public record DoctorSearchDto(Guid DoctorId, string Name, SpecialtySearchDto Specialty);
    public record SpecialtySearchDto(Guid SpecialtyId, string Name);
}