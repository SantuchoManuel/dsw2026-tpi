using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Services
{
    public class DisponibilidadService : IDisponibilidadService
    {
        private readonly IPersistence _persistence;

        public DisponibilidadService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public async Task<DisponibilidadModel.Request> CrearDisponibilidadAsync(DisponibilidadModel.Request peticion)
        {
            var doctor = await _persistence.GetById<Doctor>(peticion.DoctorId);
            if (doctor == null) { throw new ArgumentException("El médico indicado no existe."); }

            await GenerarDisponibilidades(peticion.DoctorId, peticion.Days);
            return peticion;
        }

        public async Task<DisponibilidadModel.Request> ActualizarDisponibilidadAsync(DisponibilidadModel.Request peticion)
        {
            var doctor = await _persistence.GetById<Doctor>(peticion.DoctorId);
            if (doctor == null) { throw new ArgumentException("El médico indicado no existe."); }

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
            return peticion;
        }

        private async Task GenerarDisponibilidades(Guid doctorId, List<DisponibilidadModel.EsquemaDia> diasPedidos)
        {
            var hoy = DateTime.Now.Date;
            var anioActual = hoy.Year;
            var mesActual = hoy.Month;
            var cantidadDiasDelMes = DateTime.DaysInMonth(anioActual, mesActual);

            foreach (var diaRequerido in diasPedidos)
            {
                var horaDeInicio = TimeOnly.Parse(diaRequerido.StartTime);
                var horaDeFin = TimeOnly.Parse(diaRequerido.EndTime);
                var diaDeLaSemana = MapearDiaSemana(diaRequerido.Day);

                if (horaDeInicio >= horaDeFin)
                    throw new ArgumentException($"Para el día {diaRequerido.Day}, la hora de inicio debe ser menor a la de fin.");

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
                _ => throw new ArgumentException($"El día ingresado no es válido: {dia}")
            };
        }
    }
}