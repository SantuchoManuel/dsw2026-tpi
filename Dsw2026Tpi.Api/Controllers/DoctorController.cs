using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("doctors")]

public class DoctorController : AppController
{
    private readonly IDoctorService _doctorService;

    public DoctorController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 1, [FromQuery] string? name = null)
    {
        var result = await _doctorService.GetAll(pageSize, pageIndex, name);
        return Ok(result);
    }

    [HttpGet("{id:guid}/availabilities")]
    [Authorize]

    public async Task<IActionResult> GetAvailabilities(Guid id)
    {
        try
        {
            var result = await _doctorService.GetAvailabilities(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminPolicy)]
    public async Task<IActionResult> Create([FromBody] DoctorModel.Request request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _doctorService.Create(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    public async Task<IActionResult> Update(Guid id, [FromBody] DoctorModel.Request request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _doctorService.Update(id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _doctorService.Delete(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
