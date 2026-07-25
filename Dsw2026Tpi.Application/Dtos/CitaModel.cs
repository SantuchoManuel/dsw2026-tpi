using System;

namespace Dsw2026Tpi.Application.Dtos;

public record CitaModel
{
    public record Request(Guid DoctorId, Guid AvailabilityId, PacienteDto Patient, string Reason);

    public record PacienteDto(int Dni);

    public record Response(Guid Id, DateTime Fecha, string DoctorName, string Reason);

    public record BusquedaResponse(string Specialty, string Doctor, string AvailableTime, string? PatientName = null, string? Dni = null);
}
