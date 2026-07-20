using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Cita : EntityBase
    {
        public DateTime FechaDeAtencion { get; set; }
        public DateTime FechaDeCancelacion { get; set; }
        public CitaEstado CitaEstado { get; set; }
        public Guid? PacienteId { get; set; }
        public Paciente Paciente { get; set; }
        public Guid? TurnoId { get; set; }
        public Turno Turno { get; set; }
        private Cita()
        {

        }
        public Cita(DateTime fechaDeAtencion, DateTime fechaDeCancelacion, Guid? id) : base(id)
        {
            FechaDeAtencion = fechaDeAtencion;
            FechaDeCancelacion = fechaDeCancelacion;
        }
    }
}
