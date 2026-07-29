using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dsw2026Tpi.Application.Services
{
    public class DisponibilidadService : IDisponibilidadService
    {
        private readonly IPersistence _persistence;
        private readonly ILogger<DisponibilidadService> _logger;
        private readonly IFeriadoService _feriadoService;
        private readonly TimeProvider _timeProvider;

        public DisponibilidadService(
            IPersistence persistence,
            ILogger<DisponibilidadService> logger,
            IFeriadoService feriadoService,
            TimeProvider timeProvider)
        {
            _persistence = persistence;
            _logger = logger;
            _feriadoService = feriadoService;
            _timeProvider = timeProvider;
        }
        public DisponibilidadService(IPersistence persistence, ILogger<DisponibilidadService> logger)
        {
            _persistence = persistence;
            _logger = logger;
        }

        public async Task<DisponibilidadModel.Request> CrearDisponibilidadAsync(DisponibilidadModel.Request peticion)
        {
            ValidarPeticion(peticion);

            var doctor = await _persistence.GetById<Doctor>(peticion.DoctorId);
            if (doctor == null) { throw new EntityNotFoundException("Médico").WithDetail("Medico", "No encontrado"); }

            await GenerarDisponibilidades(peticion.DoctorId, peticion.Days);

            _logger.LogInformation("Se generaron nuevas disponibilidades para el doctor con ID {DoctorId}.", peticion.DoctorId);

            return peticion;
        }

        public async Task<DisponibilidadModel.Request> ActualizarDisponibilidadAsync(DisponibilidadModel.Request peticion)
        {
            ValidarPeticion(peticion);

            var doctor = await _persistence.GetById<Doctor>(peticion.DoctorId);
            if (doctor == null) { throw new EntityNotFoundException("Médico").WithDetail("Medico", "No encontrado"); }

            var mesActual = DateTime.Now.Month;
            var anioActual = DateTime.Now.Year;

            var disponibilidadesViejas = await _persistence.GetFiltered<Disponibilidad>(r =>
                r.DoctorId == peticion.DoctorId &&
                r.Mes == mesActual &&
                r.Año == anioActual);

            foreach (var disponibilidad in disponibilidadesViejas)
            {
                var turnosViejos = await _persistence.GetFiltered<Turno>(s => s.DisponibilidadId == disponibilidad.Id);
                foreach (var turno in turnosViejos)
                {
                    await _persistence.Delete(turno);
                }

                await _persistence.Delete(disponibilidad);
            }

            await GenerarDisponibilidades(peticion.DoctorId, peticion.Days);

            _logger.LogInformation("Se eliminaron y regeneraron las disponibilidades para el doctor con ID {DoctorId}.", peticion.DoctorId);

            return peticion;
        }

        private static void ValidarPeticion(DisponibilidadModel.Request peticion)
        {
            if (peticion.DoctorId == Guid.Empty)
            {
                throw new ValidationException(
                    string.Format(ErrorCodes.FIELD_REQUIRED, "DoctorId"),
                    nameof(ErrorCodes.FIELD_REQUIRED)).WithDetail("DoctorId", "Es requerido");
            }

            if (peticion.Days == null || peticion.Days.Count == 0)
            {
                throw new ValidationException(
                    string.Format(ErrorCodes.FIELD_REQUIRED, "Días"),
                    nameof(ErrorCodes.FIELD_REQUIRED)).WithDetail("Días", "Es requerido");
            }
        }

        private async Task GenerarDisponibilidades(Guid doctorId, List<DisponibilidadModel.EsquemaDia> diasPedidos)
        {
            for (int i = 0; i < diasPedidos.Count; i++)
            {
                var dia1 = diasPedidos[i];
                var inicio1 = TimeOnly.Parse(dia1.StartTime);
                var fin1 = TimeOnly.Parse(dia1.EndTime);

                for (int j = i + 1; j < diasPedidos.Count; j++)
                {
                    var dia2 = diasPedidos[j];

                    if (dia1.Day.Trim().Equals(dia2.Day.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        var inicio2 = TimeOnly.Parse(dia2.StartTime);
                        var fin2 = TimeOnly.Parse(dia2.EndTime);

                        if (inicio1 < fin2 && fin1 > inicio2)
                        {
                            throw new ValidationException(
                                string.Format(ErrorCodes.FILTER_INVALID, "Dias"),
                                nameof(ErrorCodes.FILTER_INVALID)).WithDetail("Dia", "Los Horarios No son Validos, debido a su solapamiento");

                        }
                    }
                }
            }
            //var hoy = DateTime.Now.Date; este teniamos antes
            var hoy = _timeProvider.GetLocalNow().Date;
            var anioActual = hoy.Year;
            var mesActual = hoy.Month;
            var cantidadDiasDelMes = DateTime.DaysInMonth(anioActual, mesActual);

            foreach (var diaRequerido in diasPedidos)
            {
                var horaDeInicio = TimeOnly.Parse(diaRequerido.StartTime);
                var horaDeFin = TimeOnly.Parse(diaRequerido.EndTime);
                var diaDeLaSemana = MapearDiaSemana(diaRequerido.Day);

                if (horaDeInicio >= horaDeFin)
                {
                    throw new ValidationException(
                    string.Format(ErrorCodes.FILTER_INVALID, "EndTime"),
                    nameof(ErrorCodes.FILTER_INVALID)).WithDetail("EndTime", "La StartTime debe ser menor que EndTime");
                }

                var disponibilidadesExistentes = await _persistence.GetFiltered<Disponibilidad>(d =>
                    d.DoctorId == doctorId &&
                    d.Mes == mesActual &&
                    d.Año == anioActual &&
                    d.DiaDeLaSemana == diaDeLaSemana &&
                    d.HoraDeEntrada == horaDeInicio &&
                    d.HoraDeSalida == horaDeFin);

                if (disponibilidadesExistentes.Any())
                {
                    continue;
                }

                var nuevaDisponibilidad = new Disponibilidad(
                    mesActual,
                    anioActual,
                    diaDeLaSemana,
                    horaDeInicio,
                    horaDeFin,
                    doctorId
                );

                await _persistence.Add(nuevaDisponibilidad);

                for (int numeroDia = hoy.Day; numeroDia <= cantidadDiasDelMes; numeroDia++)
                {
                    var fechaActual = new DateTime(anioActual, mesActual, numeroDia);

                    if (fechaActual.DayOfWeek == diaDeLaSemana)
                    {
                        if (_feriadoService.EsFeriado(fechaActual))
                        {
                            _logger.LogInformation("Día omitido: No se generaron turnos para el {Fecha} por ser feriado nacional.", fechaActual.ToString("dd/MM/yyyy"));
                            continue; 
                        }
                        var relojInterno = horaDeInicio;

                        while (relojInterno < horaDeFin)
                        {
                            var nuevoTurno = new Turno(
                                DateOnly.FromDateTime(fechaActual),
                                relojInterno,
                                relojInterno.AddMinutes(30),
                                nuevaDisponibilidad.Id
                            );

                            await _persistence.Add(nuevoTurno);
                            relojInterno = relojInterno.AddMinutes(30);
                        }
                    }
                }
            }
        }

        private DayOfWeek MapearDiaSemana(string dia)
        {
            return dia.Trim().ToLower() switch
            {
                "lunes" or "monday" => DayOfWeek.Monday,
                "martes" or "tuesday" => DayOfWeek.Tuesday,
                "miercoles" or "miércoles" or "wednesday" => DayOfWeek.Wednesday,
                "jueves" or "thursday" => DayOfWeek.Thursday,
                "viernes" or "friday" => DayOfWeek.Friday,
                "sabado" or "sábado" or "saturday" => DayOfWeek.Saturday,
                "domingo" or "sunday" => DayOfWeek.Sunday,
                _ => throw new ValidationException(string.Format(ErrorCodes.FILTER_INVALID, "Dia"), nameof(ErrorCodes.FILTER_INVALID)).WithDetail("Dia", "No es Valido")
            };
        }
    }
}