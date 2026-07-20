using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Disponibilidad : EntityBase
    {
        // Cambiar tipo de HOra y dia si encontramos
        public int Mes {  get; set; }
        public int Año { get; set; }
        public DayOfWeek DiaDeLaSemana { get; set; }
        public DateTime HoraDeEntrada { get; set; }
        public DateTime HoraDeSalida { get; set; }
        public Guid? DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        private Disponibilidad()
        {
            
        }
        public Disponibilidad(int mes, int año, DayOfWeek diaDeLaSemana, DateTime horaDeEntrada, DateTime horaDeSalida, Guid? id) : base(id)
        {
            Mes = mes;
            Año = año;
            DiaDeLaSemana = diaDeLaSemana; 
            HoraDeEntrada = horaDeEntrada;
            HoraDeSalida = horaDeSalida;
        }
    }
}
