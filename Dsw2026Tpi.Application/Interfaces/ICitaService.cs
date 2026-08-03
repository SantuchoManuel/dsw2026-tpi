using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface ICitaService
    {
        Task<CitaModel.Response> CrearCitaAsync(CitaModel.Request request); Task<List<CitaModel.Response>> ObtenerTurnosPacienteAsync(int dni);
        Task CancelarCitaAsync(Guid citaId);
        Task<IEnumerable<CitaModel.BusquedaResponse>> GetAppointmentsByDateAsync(DateTime date);
        Task<Pagination<CitaModel.SearchResponse>> SearchAppointmentsAsync(Guid? specialtyId, Guid? doctorId, int? dni, DateTime? date, int pageIndex, int pageSize);
    }
}