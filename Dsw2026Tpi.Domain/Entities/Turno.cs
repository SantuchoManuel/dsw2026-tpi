using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{

    public class Turno : EntityBase
    {
        public DateOnly Fecha { get; set; }
        public TimeOnly HoraDeInicio { get; set; }
        public TimeOnly HoraDeFin { get; set; }
        public EstadoTurno EstadoTurno { get; set; }
        public Guid? DisponibilidadId { get; set; }
        public Disponibilidad Disponibilidad { get; set; }
        private Turno()
        {

        }
        public Turno(DateOnly fecha, TimeOnly horaDeInicio, TimeOnly horaDeFin, Guid? disponibilidadId, Guid? id = null) : base(id)
        {
            Fecha = fecha;
            HoraDeInicio = horaDeInicio;
            HoraDeFin = horaDeFin;
            DisponibilidadId = disponibilidadId;
            EstadoTurno = 0;
        }
    }
}

