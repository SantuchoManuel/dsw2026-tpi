using Dsw2026Tpi.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using static Dsw2026Tpi.Application.Dtos.DisponibilidadModel;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IDisponibilidadService
    {
        Task<DisponibilidadModel.Request> CrearDisponibilidadAsync(DisponibilidadModel.Request peticion);
        Task<DisponibilidadModel.Request> ActualizarDisponibilidadAsync(DisponibilidadModel.Request peticion);
    }
}
