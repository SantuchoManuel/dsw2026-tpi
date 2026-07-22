using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await _service.CrearDisponibilidadAsync(request);
            return Ok();
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromBody] DisponibilidadModel.Request request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await _service.ActualizarDisponibilidadAsync(request);
            return Ok();
        }
    }
}
