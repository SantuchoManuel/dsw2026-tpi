using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Turno : EntityBase
    {
        // Q tipo es la hora? CAMBIAR
        public string Fecha { get; set; }
        public DateTime HoraDeInicio { get; set; }
        public DateTime HoraDeFin {  get; set; }
        public EstadoTurno EstadoTurno { get; set; }
        public Guid? DisponibilidadId { get; set; }
        public Disponibilidad Disponibilidad { get; set; }
        private Turno()
        {
            
        }
        public Turno(string fecha, DateTime horaDeInicio, DateTime horaDeFin, Guid? id) : base(id)
        { 
            Fecha = fecha;
            HoraDeInicio = horaDeInicio;
            HoraDeFin = horaDeFin;
            EstadoTurno = 0;
        }
    }
}
