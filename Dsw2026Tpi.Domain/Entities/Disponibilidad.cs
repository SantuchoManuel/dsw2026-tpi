using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Disponibilidad : DeleteEntityBase
    {
        public int Mes { get; set; }
        public int Año { get; set; }
        public DayOfWeek DiaDeLaSemana { get; set; }
        public TimeOnly HoraDeEntrada { get; set; }
        public TimeOnly HoraDeSalida { get; set; }
        public Guid? DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        private Disponibilidad() 
        {
            
        }

        public Disponibilidad(int mes, int año, DayOfWeek diaDeLaSemana, TimeOnly horaDeEntrada, TimeOnly horaDeSalida, Guid? doctorId, Guid? id = null) : base(id)
        {
            Mes = mes;
            Año = año;
            DiaDeLaSemana = diaDeLaSemana;
            HoraDeEntrada = horaDeEntrada;
            HoraDeSalida = horaDeSalida;
            DoctorId = doctorId;
        }
    }
}
