using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers
{
    [Authorize(Policy = Policies.AdminPolicy)]
    [Route("availabilities")]
    public class DisponibilidadController : AppController
    {
        private readonly IDisponibilidadService _service;

        public DisponibilidadController(IDisponibilidadService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] DisponibilidadModel.Request request)
        {
            // cuando ponemos "" en doctorid tira nohadle error, esto es porque el guid no puede ser vacio, pero si es null o vacio tira error de validacion
            await _service.CrearDisponibilidadAsync(request);
            return Ok();
            /*
                    Esto no esta implementado Importante Verlo 

            ● El backend debe crear en la base de datos un registro por cada intervalo de
            30min dentro del rango de startTime y endTime.
            ● Ejemplo: si se indica que el doctor atiende Lunes desde las 9am hasta las 12pm,
            el sistema debe registrar para cada Lunes del mes turnos de 30 minutos entre
            las 9am y 12pm.
             */

        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromBody] DisponibilidadModel.Request request)
        {
            await _service.ActualizarDisponibilidadAsync(request);
            return Ok();
        }
    }
}
