using System;
using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IFeriadoService
    {
        IEnumerable<FeriadoModel> ObtenerFeriados();
        bool EsFeriado(DateTime fecha);
    }
}
