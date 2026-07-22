using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record CitaModel
    {
        public record Request(
           [Required(ErrorMessage = "El DoctorId es obligatorio.")]
            Guid DoctorId,
           [Required(ErrorMessage = "El AvailabilityId es obligatorio.")]
            Guid AvailabilityId,
           [Required(ErrorMessage = "Los datos del paciente son obligatorios.")]
            PacienteDto Patient,
           [Required(ErrorMessage = "El motivo es obligatorio.")]
            [StringLength(500, MinimumLength = 5, ErrorMessage = "El motivo debe tener entre 5 y 500 caracteres.")]
            string Reason
       );
        public record PacienteDto(
            [Range(1000000, 99999999, ErrorMessage = "El DNI debe tener entre 7 y 8 dígitos.")]
            int Dni
        );
        public record Response(Guid Id, DateTime Fecha, string DoctorName, string Reason);
    }
}
