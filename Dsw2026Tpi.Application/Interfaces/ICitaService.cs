using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface ICitaService
    {
        Task CrearCitaAsync(CitaModel.Request peticion);
        Task<List<CitaModel.Response>> ObtenerTurnosPacienteAsync(int dni);
        Task CancelarCitaAsync(Guid citaId);
        Task<IEnumerable<CitaBusquedaModel>> GetAppointmentsByDateAsync(DateTime date);
        Task<Pagination<CitaBusquedaModel>> SearchAppointmentsAsync(Guid? specialtyId, Guid? doctorId, int? dni, DateTime? date, int page, int pageSize);
    }
}
