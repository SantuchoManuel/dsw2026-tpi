using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Paciente : DeleteEntityBase
    {
        public int Dni { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Cellnumber { get; set; }
        private Paciente()
        {
            
        }
        public Paciente(int dni, string email, string name = "", string cellnumber = "", Guid? id = null) : base(id)
        { 
            Dni = dni;
            Email = email;
            Name = name;
            Cellnumber = cellnumber;
        }

    }
}
