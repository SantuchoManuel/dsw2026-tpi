using System.Text.Json;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;

namespace Dsw2026Tpi.Application.Services
{
    public class FeriadoService : IFeriadoService
    {
        private readonly IEnumerable<FeriadoModel> _feriados;

        public FeriadoService()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sources", "feriados.json");

            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                _feriados = JsonSerializer.Deserialize<IEnumerable<FeriadoModel>>(json) ?? new List<FeriadoModel>();
            }
            else
            {
                _feriados = new List<FeriadoModel>();
            }
        }

        public IEnumerable<FeriadoModel> ObtenerFeriados()
        {
            return _feriados;
        }
        public bool EsFeriado(DateTime fecha)
        {
            return _feriados.Any(f => f.Fecha.Date == fecha.Date);
        }
    }
}
