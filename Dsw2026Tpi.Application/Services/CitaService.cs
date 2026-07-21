using Microsoft.EntityFrameworkCore;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dsw2026Tpi.Application.Services
{
    public class CitaService : ICitaService
    {
        private readonly IPersistence _persistence;

        public CitaService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public async Task CrearCitaAsync(CitaModel.Request peticion)
        {
            var doctor = await _persistence.GetById<Doctor>(peticion.DoctorId);
            if (doctor == null) throw new ArgumentException("El médico no existe.");

            var turno = await _persistence.GetById<Turno>(peticion.AvailabilityId);
            if (turno == null) throw new ArgumentException("El turno indicado no existe.");

            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var horaActual = TimeOnly.FromDateTime(DateTime.Now);

            if (turno.Fecha < hoy || (turno.Fecha == hoy && turno.HoraDeInicio <= horaActual))
                throw new InvalidOperationException("No se permiten reservar turnos en el pasado.");

            if ((int)turno.EstadoTurno != 0)
                throw new InvalidOperationException("El turno ya no está disponible. Fue reservado por otro paciente.");

            var pacientes = await _persistence.GetFiltered<Paciente>(p => p.Dni == peticion.Patient.Dni);
            var paciente = pacientes.FirstOrDefault();

            if (paciente == null)
            {
                paciente = new Paciente(peticion.Patient.Dni, "Sin Email", "Sin Nombre", "Sin Celular");
                await _persistence.Add(paciente);
            }

            var nuevaCita = new Cita(DateTime.Now, DateTime.MinValue, null)
            {
                PacienteId = paciente.Id,
                TurnoId = turno.Id,
                CitaEstado = CitaEstado.Confirmada,
                Motivo = peticion.Reason 
            };


            turno.EstadoTurno = EstadoTurno.BOOKED;

            await _persistence.Add(nuevaCita);
            await _persistence.Update(turno);
        }

        public async Task<List<CitaModel.Response>> ObtenerTurnosPacienteAsync(int dni)
        {
            var pacientes = await _persistence.GetFiltered<Paciente>(p => p.Dni == dni);
            var paciente = pacientes.FirstOrDefault();
            if (paciente == null) throw new ArgumentException("Paciente no encontrado.");

            var citas = await _persistence.GetFiltered<Cita>(
                c => c.PacienteId == paciente.Id && (int)c.CitaEstado == 0,
                "Turno", "Turno.Disponibilidad", "Turno.Disponibilidad.Doctor"
            );

            var turnosResponse = new List<CitaModel.Response>();
            foreach (var cita in citas)
            {
                var fechaYHora = cita.Turno.Fecha.ToDateTime(cita.Turno.HoraDeInicio);

                turnosResponse.Add(new CitaModel.Response(
                    cita.Id,
                    fechaYHora,
                    cita.Turno.Disponibilidad.Doctor.Name, 
                    cita.Motivo 
                ));
            }

            return turnosResponse;
        }

        public async Task CancelarCitaAsync(Guid citaId)
        {
            var cita = await _persistence.GetById<Cita>(citaId, "Turno");
            if (cita == null) throw new ArgumentException("La cita no existe.");

            if ((int)cita.Turno.EstadoTurno != 1)
                throw new InvalidOperationException("Solo se pueden cancelar turnos que estén reservados (BOOKED).");

            cita.FechaDeCancelacion = DateTime.Now;
            cita.CitaEstado = (CitaEstado)1; 

            cita.Turno.EstadoTurno = EstadoTurno.AVAILABLE;

            await _persistence.Update(cita);
            await _persistence.Update(cita.Turno);
        }
        public async Task<IEnumerable<CitaBusquedaModel>> GetAppointmentsByDateAsync(DateTime date)
        {
            var targetDate = DateOnly.FromDateTime(date);

            var citas = await _persistence.GetFiltered<Cita>(
                c => c.Turno.Fecha == targetDate, "Turno.Disponibilidad.Doctor.Speciality" 
            );

            return citas.Select(c => new CitaBusquedaModel
            {
                Specialty = c.Turno.Disponibilidad.Doctor.Speciality.Name ?? c.Turno.Disponibilidad.Doctor.Speciality.Name,
                Doctor = c.Turno.Disponibilidad.Doctor.Name ?? c.Turno.Disponibilidad.Doctor.Name,
                AvailableTime = $"{c.Turno.Fecha:yyyy-MM-dd} {c.Turno.HoraDeInicio}"
            }).ToList();
        }

        public async Task<Pagination<CitaBusquedaModel>> SearchAppointmentsAsync(
            Guid? specialtyId, Guid? doctorId, int? dni, DateTime? date, int page, int pageSize)
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
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CitaBusquedaModel
                {
                    Specialty = c.Turno.Disponibilidad.Doctor.Speciality.Name ?? c.Turno.Disponibilidad.Doctor.Speciality.Name,
                    Doctor = c.Turno.Disponibilidad.Doctor.Name ?? c.Turno.Disponibilidad.Doctor.Name,
                    AvailableTime = $"{c.Turno.Fecha:yyyy-MM-dd} {c.Turno.HoraDeInicio}",
                    PatientName = c.Paciente.Name,
                    Dni = c.Paciente.Dni.ToString()
                })
                .ToList();

            return new Pagination<CitaBusquedaModel>(pageSize, page, totalRecords, items);
        }
    }
}