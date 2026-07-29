using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Dsw2026Tpi.Application.Dtos
{
    public class FeriadoModel
    {
        [JsonPropertyName("fecha")]
        public DateTime Fecha { get; set; }

        [JsonPropertyName("dia_semana")]
        public string DiaSemana { get; set; }

        [JsonPropertyName("motivo")]
        public string Motivo { get; set; }

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; }
    }
}
