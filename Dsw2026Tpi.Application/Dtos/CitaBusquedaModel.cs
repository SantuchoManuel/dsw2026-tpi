using System;
using System.Collections.Generic;
using System.Text;
namespace Dsw2026Tpi.Application.Dtos

{
    public class CitaBusquedaModel
    {
        public string Specialty { get; set; } = string.Empty;
        public string Doctor { get; set; } = string.Empty;
        public string AvailableTime { get; set; } = string.Empty;
        public string? PatientName { get; set; }
        public string? Dni { get; set; }
    }
}
