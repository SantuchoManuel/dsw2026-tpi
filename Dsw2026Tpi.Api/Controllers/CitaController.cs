using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers
{
    [Route("appointments")]
    public class CitaController : AppController
    {
        private readonly ICitaService _service;

        public CitaController(ICitaService service)
        {
            _service = service;
        }

        [Authorize(Policy = Policies.PatientPolicy)]
        [HttpPost]
        public async Task<IActionResult> SolicitarTurno([FromBody] CitaModel.Request request)
        {
            if (request.DoctorId == Guid.Empty)
            {
                return BadRequest("El DoctorId es obligatorio.");
            }
            if (request.AvailabilityId == Guid.Empty)
            {
                return BadRequest("El AvailabilityId es obligatorio.");
            }

            var dniString = request.Patient?.Dni.ToString();
            if (string.IsNullOrEmpty(dniString) || dniString.Length < 7 || dniString.Length > 10)
            {
                return BadRequest("DNI obligatorio y debe tener entre 7 y 10 dígitos.");
            }
                
            if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length < 5)
            {
                return BadRequest("El motivo (reason) es obligatorio y debe tener al menos 5 caracteres.");
            }
            
            await _service.CrearCitaAsync(request);

            return Ok();
        }

        [Authorize(Policy = Policies.PatientPolicy)]
        [HttpGet("patient")]
        public async Task<IActionResult> VerTurnosPaciente([FromQuery] int dni)
        {
            if (dni <= 0)
            {
                return BadRequest("DNI inválido.");
            } 
            var turnos = await _service.ObtenerTurnosPacienteAsync(dni);
            return Ok(turnos);
        }

        [Authorize(Policy = Policies.PatientPolicy)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelarTurno(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("ID de cita inválido.");
            }
            await _service.CancelarCitaAsync(id);
            return Ok();
        }
    }
}
