using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Dsw2026Tpi.Application.Dtos;

public record SpecialityModel
{
    public record Request(
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        string Name,
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 100 caracteres.")]
        string Description
    );
    public record Response(Guid Id, string Name, string Description);
}

/*

Especialidad
Response
{
"pageSize": 0,
"pageIndex": 0,
"data": [
        {
        "id": "Guid",
        "name": "string",
        "description”: "string"
        },
        ],
"total": number
}


 */