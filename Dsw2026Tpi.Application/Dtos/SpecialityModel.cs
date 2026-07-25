using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Dsw2026Tpi.Application.Dtos;

public record SpecialityModel
{
    public record Request(string Name, string Description);
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