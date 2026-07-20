using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Paciente : EntityBase
    {
        public int Dni { get; set; }
        public string Name { get; set; }
        public string Cellnumber { get; set; }
        private Paciente()
        {
            
        }
        public Paciente(int dni, string name, string cellnumber, Guid? id = null) : base(id)
        { 
            Dni = dni;
            Name = name;
            Cellnumber = cellnumber;
        }

    }
}
