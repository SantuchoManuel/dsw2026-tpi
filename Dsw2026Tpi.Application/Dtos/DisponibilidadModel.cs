using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record DisponibilidadModel
    {
        public record Request(Guid DoctorId, List<EsquemaDia> Days);
        public record EsquemaDia(string Day, string StartTime, string EndTime);
    }
}
