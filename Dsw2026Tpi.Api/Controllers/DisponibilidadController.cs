using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers
{
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
            if (request.DoctorId == System.Guid.Empty)
                return BadRequest("El DoctorId es obligatorio.");

            if (request.Days == null || request.Days.Count == 0)
                return BadRequest("Debe enviar al menos un día.");

            await _service.CrearDisponibilidadAsync(request);

            return Ok();
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromBody] DisponibilidadModel.Request request)
        {
            if (request.DoctorId == System.Guid.Empty)
                return BadRequest("El DoctorId es obligatorio.");

            if (request.Days == null || request.Days.Count == 0)
                return BadRequest("Debe enviar al menos un día.");

            await _service.ActualizarDisponibilidadAsync(request);

            return Ok();
        }
    }
}
