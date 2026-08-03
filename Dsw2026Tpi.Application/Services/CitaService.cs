using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Dsw2026Tpi.CrossCutting.Resources;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dsw2026Tpi.Application.Services;

public class CitaService : ICitaService
{
    private readonly IPersistence _persistence;
    private readonly ILogger<CitaService> _logger;

    public CitaService(IPersistence persistence, ILogger<CitaService> logger)
    {
        _persistence = persistence;
        _logger = logger;
    }

    public async Task<CitaModel.Response> CrearCitaAsync(CitaModel.Request request)
    {
        ValidateRequest(request);

        var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
        if (doctor == null) throw new EntityNotFoundException("Doctor").WithDetail("DoctorId", "No Encontrado");

        var turno = await _persistence.GetById<Turno>(request.AvailabilityId);
        if (turno == null) throw new EntityNotFoundException("Turno").WithDetail("AvailabilityId", "No Encontrado");

        var hoy = DateOnly.FromDateTime(DateTime.Now);
        var horaActual = TimeOnly.FromDateTime(DateTime.Now);

        if (turno.Fecha < hoy || (turno.Fecha == hoy && turno.HoraDeInicio <= horaActual))
            throw new ValidationException(ErrorCodes.TURNO_PASADO, nameof(ErrorCodes.TURNO_PASADO)).WithDetail("DateTime", "Slot invalid");

        if ((int)turno.EstadoTurno != 0)
            throw new ConflictException(nameof(ErrorCodes.APPOINTMENT_CONFLICT), ErrorCodes.APPOINTMENT_CONFLICT).WithDetail("AvailabilityId", "El turno ya no está disponible.");

        var pacientes = await _persistence.GetFiltered<Paciente>(p => p.Dni == request.Patient.Dni);
        var paciente = pacientes.FirstOrDefault();

        if (paciente == null)
        {
            paciente = new Paciente(request.Patient.Dni, "Sin Email", "Sin Nombre", "Sin Celular");
            await _persistence.Add(paciente);
            _logger.LogInformation("Paciente creado automáticamente durante la reserva. DNI: {Dni}", request.Patient.Dni);
        }

        var nuevaCita = new Cita(DateTime.Now, DateTime.MinValue, null)
        {
            PacienteId = paciente.Id,
            TurnoId = turno.Id,
            CitaEstado = CitaEstado.Confirmada,
            Motivo = request.Reason
        };

        turno.EstadoTurno = EstadoTurno.BOOKED;

        await _persistence.Add(nuevaCita);
        await _persistence.Update(turno);

        _logger.LogInformation("Turno {TurnoId} reservado exitosamente por el paciente DNI {Dni}.", turno.Id, request.Patient.Dni);

        return new CitaModel.Response(
        nuevaCita.Id,
        turno.Fecha.ToDateTime(turno.HoraDeInicio),
        doctor.Name,
        nuevaCita.Motivo
    );
    }

    public async Task<List<CitaModel.Response>> ObtenerTurnosPacienteAsync(int dni)
    {
        if (dni <= 0)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_INVALID, "Dni"),
                nameof(ErrorCodes.FIELD_INVALID)
            ).WithDetail("dni", "Debe ser mayor a cero");
        }

        var pacientes = await _persistence.GetFiltered<Paciente>(p => p.Dni == dni);
        var paciente = pacientes.FirstOrDefault();
        if (paciente == null) throw new EntityNotFoundException("Paciente").WithDetail("Paciente", "No Encontrado");

        var citas = await _persistence.GetFiltered<Cita>(
            c => c.PacienteId == paciente.Id && (int)c.CitaEstado == 0,
            "Turno", "Turno.Disponibilidad", "Turno.Disponibilidad.Doctor"
        );

        return citas.Select(cita => new CitaModel.Response(
            cita.Id,
            cita.Turno.Fecha.ToDateTime(cita.Turno.HoraDeInicio),
            cita.Turno.Disponibilidad.Doctor.Name,
            cita.Motivo
        )).ToList();
    }

    public async Task CancelarCitaAsync(Guid id)
    {
        var cita = await _persistence.GetById<Cita>(id, "Turno");
        if (cita == null) throw new EntityNotFoundException("Cita").WithDetail("Cita", "No Encontrada");

        if ((int)cita.Turno.EstadoTurno != 1)
            throw new ConflictException(nameof(ErrorCodes.ESTADO_INVALIDO), ErrorCodes.ESTADO_INVALIDO).WithDetail("EstadoTurno", "Estado no válido para cancelación");

        cita.FechaDeCancelacion = DateTime.Now;
        cita.CitaEstado = (CitaEstado)1;

        cita.Turno.EstadoTurno = EstadoTurno.AVAILABLE;

        await _persistence.Update(cita);
        await _persistence.Update(cita.Turno);

        _logger.LogInformation("La cita {CitaId} fue cancelada exitosamente. Turno liberado.", id);
    }

    public async Task<IEnumerable<CitaModel.BusquedaResponse>> GetAppointmentsByDateAsync(DateTime date)
    {
        var targetDate = DateOnly.FromDateTime(date);

        var citas = await _persistence.GetFiltered<Cita>(
            c => c.Turno.Fecha == targetDate, "Turno.Disponibilidad.Doctor.Speciality"
        );

        return citas.Select(c => new CitaModel.BusquedaResponse(
            c.Turno.Disponibilidad.Doctor.Speciality.Name ?? c.Turno.Disponibilidad.Doctor.Speciality.Name,
            c.Turno.Disponibilidad.Doctor.Name ?? c.Turno.Disponibilidad.Doctor.Name,
            $"{c.Turno.Fecha:yyyy-MM-dd} {c.Turno.HoraDeInicio}"
        )).ToList();
    }

    public async Task<Pagination<CitaModel.BusquedaResponse>> SearchAppointmentsAsync(
        Guid? specialtyId, Guid? doctorId, int? dni, DateTime? date, int pageIndex, int pageSize)
    {
        DateOnly? targetDate = date.HasValue ? DateOnly.FromDateTime(date.Value) : null;

        var citas = await _persistence.GetFiltered<Cita>(
            c => (!targetDate.HasValue || c.Turno.Fecha == targetDate.Value) &&
                 (!specialtyId.HasValue || c.Turno.Disponibilidad.Doctor.Speciality.Id == specialtyId.Value) &&
                 (!doctorId.HasValue || c.Turno.Disponibilidad.Doctor.Id == doctorId.Value) &&
                 (!dni.HasValue || c.Paciente.Dni == dni.Value),
            "Turno.Disponibilidad.Doctor.Speciality", "Paciente"
        );

        var totalRecords = citas.Count();

        var items = citas
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CitaModel.BusquedaResponse(
                c.Turno.Disponibilidad.Doctor.Speciality.Name ?? c.Turno.Disponibilidad.Doctor.Speciality.Name,
                c.Turno.Disponibilidad.Doctor.Name ?? c.Turno.Disponibilidad.Doctor.Name,
                $"{c.Turno.Fecha:yyyy-MM-dd} {c.Turno.HoraDeInicio}",
                c.Paciente.Name,
                c.Paciente.Dni.ToString()
            ))
            .ToList();

        return new Pagination<CitaModel.BusquedaResponse>(pageSize, pageIndex, totalRecords, items);
    }

    private static void ValidateRequest(CitaModel.Request request)
    {
        if (request.DoctorId == Guid.Empty)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_REQUIRED, "DoctorId"),
                nameof(ErrorCodes.FIELD_REQUIRED)
            ).WithDetail("DoctorId", "Es requerido");
        }

        if (request.AvailabilityId == Guid.Empty)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_REQUIRED, "AvailabilityId"),
                nameof(ErrorCodes.FIELD_REQUIRED)
            ).WithDetail("AvailabilityId", "Es requerido");
        }

        if (request.Patient == null || request.Patient.Dni.ToString().Length < 7 || request.Patient.Dni.ToString().Length > 10)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_INVALID, "Patient.Dni"),
                nameof(ErrorCodes.FIELD_INVALID)
            ).WithDetail("Patient.Dni", "Longitud es inválida");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_REQUIRED, "Reason"),
                nameof(ErrorCodes.FIELD_REQUIRED)
            ).WithDetail("Reason", "Es requerido");
        }

        if (request.Reason.Length < 5 || request.Reason.Length > 500)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_LENGTH_INVALID, "Reason", 5, 500),
                nameof(ErrorCodes.FIELD_LENGTH_INVALID)
            ).WithDetail("Reason", "La longitud es inválida");
        }
    }

}