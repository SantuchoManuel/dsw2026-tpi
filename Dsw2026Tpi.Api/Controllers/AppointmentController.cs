using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Threading.Tasks;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/appointments")]
public class AppointmentController : AppController
{
    private readonly IAppointmentService _service;

    public AppointmentController(IAppointmentService service)
    {
        _service = service;
    }

    [EnableRateLimiting("AppointmentsPolicy")]
    [Authorize(Policy = Policies.PatientPolicy)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BookAppointment([FromBody] AppointmentModel.Request request)
    {
        var result = await _service.CreateAppointmentAsync(request);
        return Ok(result);
    }

    [Authorize(Policy = Policies.PatientPolicy)]
    [HttpGet("patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientAppointments([FromQuery] int dni)
    {
        var appointments = await _service.GetPatientAppointmentsAsync(dni);
        return Ok(appointments);
    }

    [Authorize(Policy = Policies.PatientPolicy)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelAppointment(Guid id)
    {
        await _service.CancelAppointmentAsync(id);
        return Ok("ok");
    }

    [Authorize(Roles = Roles.Administrator)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppointmentsByDate([FromQuery] DateTime date)
    {
        var result = await _service.GetAppointmentsByDateAsync(date);
        return Ok(result);
    }

    [Authorize(Roles = Roles.Administrator)]
    [HttpGet("search")]
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