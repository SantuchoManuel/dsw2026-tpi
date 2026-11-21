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
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IPersistence _persistence;
        private readonly ILogger<AvailabilityService> _logger;
        private readonly IHolidayService _holidayService;
        private readonly TimeProvider _timeProvider;

        public AvailabilityService(
            IPersistence persistence,
            ILogger<AvailabilityService> logger,
            IHolidayService holidayService,
            TimeProvider timeProvider)
        {
            _persistence = persistence;
            _logger = logger;
            _holidayService = holidayService;
            _timeProvider = timeProvider;
        }
        public AvailabilityService(IPersistence persistence, ILogger<AvailabilityService> logger)
        {
            _persistence = persistence;
            _logger = logger;
        }

        public async Task<AvailabilityModel.Request> CreateAvailabilityAsync(AvailabilityModel.Request request)
        {
            ValidateRequest(request);

            var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
            if (doctor == null) { throw new EntityNotFoundException("Doctor").WithDetail("Doctor", "No encontrado"); }

            await GenerateAvailabilities(request.DoctorId, request.Days);

            _logger.LogInformation("Se generaron nuevas disponibilidades para el doctor con ID {DoctorId}.", request.DoctorId);

            return request;
        }

        public async Task<AvailabilityModel.Request> UpdateAvailabilityAsync(AvailabilityModel.Request request)
        {
            ValidateRequest(request);

            var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
            if (doctor == null) { throw new EntityNotFoundException("Doctor").WithDetail("Doctor", "No encontrado"); }

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var oldAvailabilities = await _persistence.GetFiltered<Availability>(r =>
                r.DoctorId == request.DoctorId &&
                r.Month == currentMonth &&
                r.Year == currentYear);

            foreach (var availability in oldAvailabilities)
            {
                var oldSlots = await _persistence.GetFiltered<AvailabilitySlot>(s => s.AvailabilityId == availability.Id);
                foreach (var slot in oldSlots)
                {
                    await _persistence.Delete(slot);
                }

                await _persistence.Delete(availability);
            }

            await GenerateAvailabilities(request.DoctorId, request.Days);

            _logger.LogInformation("Se eliminaron y regeneraron las disponibilidades para el doctor con ID {DoctorId}.", request.DoctorId);

            return request;
        }

        private static void ValidateRequest(AvailabilityModel.Request request)
        {
            if (request.DoctorId == Guid.Empty)
            {
                throw new ValidationException(
                    string.Format(ErrorCodes.FIELD_REQUIRED, "DoctorId"),
                    nameof(ErrorCodes.FIELD_REQUIRED)).WithDetail("DoctorId", "Es requerido");
            }

            if (request.Days == null || request.Days.Count == 0)
            {
                throw new ValidationException(
                    string.Format(ErrorCodes.FIELD_REQUIRED, "Days"),
                    nameof(ErrorCodes.FIELD_REQUIRED)).WithDetail("Days", "Es requerido");
            }
        }

        private async Task GenerateAvailabilities(Guid doctorId, List<AvailabilityModel.DaySchedule> requestedDays)
        {
            for (int i = 0; i < requestedDays.Count; i++)
            {
                var day1 = requestedDays[i];
                var start1 = TimeOnly.Parse(day1.StartTime);
                var end1 = TimeOnly.Parse(day1.EndTime);

                for (int j = i + 1; j < requestedDays.Count; j++)
                {
                    var day2 = requestedDays[j];
                    if (day1.Day.Trim().Equals(day2.Day.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        var start2 = TimeOnly.Parse(day2.StartTime);
                        var end2 = TimeOnly.Parse(day2.EndTime);
                        if (start1 < end2 && end1 > start2)
                        {
                            throw new ValidationException(
                                string.Format(ErrorCodes.FILTER_INVALID, "Days"),
                                nameof(ErrorCodes.FILTER_INVALID))
                                .WithDetail("Day", "Los Horarios No son Validos, debido a su solapamiento");
                        }
                    }
                }
            }

            var today = _timeProvider.GetLocalNow().Date;
            var currentYear = today.Year;
            var currentMonth = today.Month;
            var daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);

            var newAvailabilities = new List<Availability>();

            foreach (var requestedDay in requestedDays)
            {
                var startTime = TimeOnly.Parse(requestedDay.StartTime);
                var endTime = TimeOnly.Parse(requestedDay.EndTime);
                var dayOfWeek = MapDayOfWeek(requestedDay.Day);

                if (startTime >= endTime)
                {
                    throw new ValidationException(
                        string.Format(ErrorCodes.FILTER_INVALID, "EndTime"),
                        nameof(ErrorCodes.FILTER_INVALID))
                        .WithDetail("EndTime", "La StartTime debe ser menor que EndTime");
                }

                // Traer todas las disponibilidades del mismo doctor/mes/año/día
                // y verificar solapamiento de intervalos (no solo coincidencia exacta)
                var existingAvailabilities = await _persistence.GetFiltered<Availability>(d =>
                    d.DoctorId == doctorId &&
                    d.Month == currentMonth &&
                    d.Year == currentYear &&
                    d.DayOfWeek == dayOfWeek);

                if (existingAvailabilities.Any(d =>
                    d.StartTime < endTime && d.EndTime > startTime))
                {
                    throw new ValidationException(
                    string.Format(ErrorCodes.FILTER_INVALID, "Overlap"),
                    nameof(ErrorCodes.FILTER_INVALID)).WithDetail("Schedule", "El horario se solapa con una disponibilidad ya existente para ese día");
                }

                var newAvailability = new Availability(
                    currentMonth, currentYear, dayOfWeek, startTime, endTime, doctorId);

                for (int dayNumber = today.Day; dayNumber <= daysInMonth; dayNumber++)
                {
                    var currentDate = new DateTime(currentYear, currentMonth, dayNumber);

                    if (currentDate.DayOfWeek == dayOfWeek)
                    {
                        if (_holidayService.IsHoliday(currentDate))
                        {
                            _logger.LogInformation(
                                "Día omitido: No se generaron turnos para el {Date} por ser feriado nacional.",
                                currentDate.ToString("dd/MM/yyyy"));
                            continue;
                        }

                        var slotTime = startTime;
                        while (slotTime < endTime)
                        {
                            var newSlot = new AvailabilitySlot(
                                DateOnly.FromDateTime(currentDate),
                                slotTime,
                                slotTime.AddMinutes(30),
                                null 
                            );

                            newAvailability.AvailabilitySlots.Add(newSlot);
                            slotTime = slotTime.AddMinutes(30);
                        }
                    }
                }

                newAvailabilities.Add(newAvailability);
            }

            await _persistence.AddRange(newAvailabilities);
        }


        private DayOfWeek MapDayOfWeek(string day)
        {
            return day.Trim().ToLower() switch
            {
                "lunes" or "monday" => DayOfWeek.Monday,
                "martes" or "tuesday" => DayOfWeek.Tuesday,
                "miercoles" or "miércoles" or "wednesday" => DayOfWeek.Wednesday,
                "jueves" or "thursday" => DayOfWeek.Thursday,
                "viernes" or "friday" => DayOfWeek.Friday,
                "sabado" or "sábado" or "saturday" => DayOfWeek.Saturday,
                "domingo" or "sunday" => DayOfWeek.Sunday,
                _ => throw new ValidationException(string.Format(ErrorCodes.FILTER_INVALID, "Day"), nameof(ErrorCodes.FILTER_INVALID)).WithDetail("Day", "No es Valido")
            };
        }
    }
}