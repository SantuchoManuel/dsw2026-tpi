using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Dsw2026Tpi.Application.Dtos
{
    public record DisponibilidadModel
    {
        public record Request(Guid DoctorId, List<EsquemaDia> Days);
        public record EsquemaDia( string Day,string StartTime, string EndTime);
    }

    /*
  public record Request(
                    [Required(ErrorMessage = "El DoctorId es obligatorio.")]
            Guid DoctorId,
                    [Required(ErrorMessage = "Debe enviar al menos un día.")]
                    [MinLength(1, ErrorMessage = "Debe enviar al menos un día.")]
            List<EsquemaDia> Days
                );

    public record EsquemaDia(
                [Required(ErrorMessage = "El día es obligatorio.")]
                [StringLength(20, ErrorMessage = "El día no puede superar 20 caracteres.")]
            string Day,
                [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
                [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "StartTime debe tener formato HH:mm.")]
            string StartTime,
                [Required(ErrorMessage = "La hora de fin es obligatoria.")]
                [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "EndTime debe tener formato HH:mm.")]
            string EndTime
        );

  */
}
