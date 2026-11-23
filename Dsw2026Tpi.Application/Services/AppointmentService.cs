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

public class AppointmentService : IAppointmentService
{
    private readonly IPersistence _persistence;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(IPersistence persistence, ILogger<AppointmentService> logger)
    {
        _persistence = persistence;
        _logger = logger;
    }

    public async Task<AppointmentModel.Response> CreateAppointmentAsync(AppointmentModel.Request request)
    {
        ValidateRequest(request);

        var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
        if (doctor == null) throw new EntityNotFoundException("Doctor").WithDetail("DoctorId", "No Encontrado");

        var slot = await _persistence.GetById<AvailabilitySlot>(request.AvailabilitySlotId);
        if (slot == null) throw new EntityNotFoundException("AvailabilitySlot").WithDetail("AvailabilitySlotId", "No Encontrado");

        var today = DateOnly.FromDateTime(DateTime.Now);
        var currentTime = TimeOnly.FromDateTime(DateTime.Now);

        if (slot.Date < today || (slot.Date == today && slot.StartTime <= currentTime))
            throw new ValidationException(ErrorCodes.SLOT_IN_PAST, nameof(ErrorCodes.SLOT_IN_PAST)).WithDetail("DateTime", "Slot invalido");

        if ((int)slot.Status != 0)
            throw new ConflictException(nameof(ErrorCodes.APPOINTMENT_CONFLICT), ErrorCodes.APPOINTMENT_CONFLICT).WithDetail("AvailabilitySlotId", "El turno ya no está disponible.");

        var patient = await _persistence.First<Patient>(p => p.Dni == request.Patient.Dni);

        if (patient == null)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FILTER_INVALID, "Patient.Dni"),
                nameof(ErrorCodes.FILTER_INVALID)
            ).WithDetail("Patient.Dni", "El paciente con este DNI no existe. Debe iniciar sesión primero.");
        }

        var newAppointment = new Appointment(DateTime.Now, DateTime.MinValue, null)
        {
            PatientId = patient.Id,
            AvailabilitySlotId = slot.Id,
            Status = AppointmentStatus.Confirmed,
            Reason = request.Reason
        };

        slot.Status = SlotStatus.BOOKED;

        await _persistence.Add(newAppointment);
        await _persistence.Update(slot);

        _logger.LogInformation("Turno {SlotId} reservado exitosamente por el paciente DNI {Dni}.", slot.Id, request.Patient.Dni);

        return new AppointmentModel.Response(
        newAppointment.Id,
        slot.Date.ToDateTime(slot.StartTime),
        doctor.Name,
        newAppointment.Reason
    );
    }

    public async Task<List<AppointmentModel.Response>> GetPatientAppointmentsAsync(int dni)
    {
        if (dni <= 0)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_INVALID, "Dni"),
                nameof(ErrorCodes.FIELD_INVALID)
            ).WithDetail("dni", "Debe ser mayor a cero");
        }

        var patients = await _persistence.GetFiltered<Patient>(p => p.Dni == dni);
        var patient = patients.FirstOrDefault();
        if (patient == null) throw new EntityNotFoundException("Patient").WithDetail("Patient", "No Encontrado");

        var appointments = await _persistence.GetFiltered<Appointment>(
            c => c.PatientId == patient.Id && (int)c.Status == 0,
            "AvailabilitySlot", "AvailabilitySlot.Availability", "AvailabilitySlot.Availability.Doctor"
        );

        return appointments.Select(appointment => new AppointmentModel.Response(
            appointment.Id,
            appointment.AvailabilitySlot.Date.ToDateTime(appointment.AvailabilitySlot.StartTime),
            appointment.AvailabilitySlot.Availability.Doctor.Name,
            appointment.Reason
        )).ToList();
    }

    public async Task CancelAppointmentAsync(Guid id)
    {
        var appointment = await _persistence.GetById<Appointment>(id, "AvailabilitySlot");
        if (appointment == null) throw new EntityNotFoundException("Appointment").WithDetail("Appointment", "No Encontrada");

        if ((int)appointment.AvailabilitySlot.Status != 1)
            throw new ConflictException(nameof(ErrorCodes.INVALID_STATUS), ErrorCodes.INVALID_STATUS).WithDetail("Status", "Estado no válido para cancelación");

        appointment.CancellationDate = DateTime.Now;
        appointment.Status = (AppointmentStatus)1;

        appointment.AvailabilitySlot.Status = SlotStatus.AVAILABLE;

        await _persistence.Update(appointment);
        await _persistence.Update(appointment.AvailabilitySlot);

        _logger.LogInformation("La cita {AppointmentId} fue cancelada exitosamente. Turno liberado.", id);
    }

    public async Task<IEnumerable<AppointmentModel.ByDateResponse>> GetAppointmentsByDateAsync(DateTime date)
    {
        var targetDate = DateOnly.FromDateTime(date);

        var appointments = await _persistence.GetFiltered<Appointment>(
            c => c.AvailabilitySlot.Date == targetDate, "AvailabilitySlot.Availability.Doctor.Speciality", "Patient"
        );

        return appointments.Select(c => new AppointmentModel.ByDateResponse(
            c.AvailabilitySlot.Availability.Doctor.Speciality.Name,
            c.AvailabilitySlot.Availability.Doctor.Name,
            $"{c.AvailabilitySlot.Date:yyyy-MM-dd} {c.AvailabilitySlot.StartTime}",
            c.Patient.Name ?? "Sin Nombre", 
            c.Patient.Dni.ToString() 
        )).ToList();
    }

    public async Task<Pagination<AppointmentModel.SearchResponse>> SearchAppointmentsAsync(
         Guid? specialtyId, Guid? doctorId, int? dni, DateTime? date, int pageIndex, int pageSize)
    {
        DateOnly? targetDate = date.HasValue ? DateOnly.FromDateTime(date.Value) : null;

        var appointments = await _persistence.GetFiltered<Appointment>(
            c => (!targetDate.HasValue || c.AvailabilitySlot.Date == targetDate.Value) &&
                 (!specialtyId.HasValue || c.AvailabilitySlot.Availability.Doctor.SpecialityId == specialtyId.Value) &&
                 (!doctorId.HasValue || c.AvailabilitySlot.Availability.DoctorId == doctorId.Value) &&
                 (!dni.HasValue || c.Patient.Dni == dni.Value),
            "AvailabilitySlot.Availability.Doctor.Speciality", "Patient"
        );

        var totalRecords = appointments.Count();

        var items = appointments
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new AppointmentModel.SearchResponse(
                c.Id,
                c.AvailabilitySlot.Date.ToDateTime(c.AvailabilitySlot.StartTime),
                c.Status.ToString(),
                new AppointmentModel.PatientSearchDto(
                    c.Patient.Dni,
                    c.Patient.Name ?? string.Empty
                ),
                new AppointmentModel.DoctorSearchDto(
                    c.AvailabilitySlot.Availability.Doctor.Id,
                    c.AvailabilitySlot.Availability.Doctor.Name,
                    new AppointmentModel.SpecialtySearchDto(
                        c.AvailabilitySlot.Availability.Doctor.Speciality.Id,
                        c.AvailabilitySlot.Availability.Doctor.Speciality.Name
                    )
                )
            ))
            .ToList();

        return new Pagination<AppointmentModel.SearchResponse>(pageSize, pageIndex, totalRecords, items);
    }

    private static void ValidateRequest(AppointmentModel.Request request)
    {
        if (request.DoctorId == Guid.Empty)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_REQUIRED, "DoctorId"),
                nameof(ErrorCodes.FIELD_REQUIRED)
            ).WithDetail("DoctorId", "Es requerido");
        }

        if (request.AvailabilitySlotId == Guid.Empty)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_REQUIRED, "AvailabilitySlotId"),
                nameof(ErrorCodes.FIELD_REQUIRED)
            ).WithDetail("AvailabilitySlotId", "Es requerido");
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