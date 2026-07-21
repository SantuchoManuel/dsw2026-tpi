using Dsw2026Tpi.Application.Dtos;
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
    }
}
