using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Dsw2026Tpi.Api.Controllers;

public class CitaController : AppController
{
    private readonly ICitaService _service;

    public CitaController(ICitaService service)
    {
        _service = service;
    }

    [Authorize(Policy = Policies.PatientPolicy)]
    [HttpPost("appointments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SolicitarTurno([FromBody] CitaModel.Request request)
    {
        await _service.CrearCitaAsync(request);
        return Ok();
    }

    [Authorize(Policy = Policies.PatientPolicy)]
    [HttpGet("appointments/patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerTurnosPaciente([FromQuery] int dni)
    {
        var turnos = await _service.ObtenerTurnosPacienteAsync(dni);
        return Ok(turnos);
    }

    [Authorize(Policy = Policies.PatientPolicy)]
    [HttpDelete("appointments/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelarTurno(Guid id)
    {
        await _service.CancelarCitaAsync(id);
        return Ok();
    }

    [Authorize(Roles = Roles.Administrator)]
    [HttpGet("appointments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppointmentsByDate([FromQuery] DateTime date)
    {
        var result = await _service.GetAppointmentsByDateAsync(date);
        return Ok(result);
    }

    [Authorize(Roles = Roles.Administrator)]
    [HttpGet("appointments/search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchAppointments(
        [FromQuery] int pageSize = 10,
        [FromQuery] int pageIndex = 1,
        [FromQuery] Guid? specialtyId = null,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] int? dni = null,
        [FromQuery] DateTime? date = null)
    {
        var result = await _service.SearchAppointmentsAsync(specialtyId, doctorId, dni, date, pageIndex, pageSize);
        return Ok(result);
    }
}