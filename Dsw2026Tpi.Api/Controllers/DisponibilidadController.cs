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
    [Route("api/availabilities")]
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
            var result = await _service.CrearDisponibilidadAsync(request);
            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromBody] DisponibilidadModel.Request request)
        {
            var result = await _service.ActualizarDisponibilidadAsync(request);
            return Ok(result);
        }
    }
}