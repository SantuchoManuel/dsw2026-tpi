using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Authorize(Policy = Policies.AdminPolicy)]
public class SpecialityController : AppController
{
    private readonly ISpecialityService _service;

    public SpecialityController(ISpecialityService service)
    {
        _service = service;
    }

    [HttpGet("specialitiespageSize=&pageIndex=&name=")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 1, [FromQuery] string? name = null)
    {
        // Validaciones
        if (!string.IsNullOrEmpty(name) && (name.Length < 3 || name.Length > 100)) 
            return BadRequest("El parámetro 'name' debe tener entre 3 y 100 caracteres.");
        
        var speciality = await _service.GetAll(pageSize, pageIndex, name);
        return Ok(speciality);
    }


    [HttpPost("specialities")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] SpecialityModel.Request request)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 100)
            return BadRequest("El campo 'name' es obligatorio y debe tener entre 3 y 100 caracteres.");

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10 || request.Description.Length > 100)
            return BadRequest("El campo 'description' es obligatorio y debe tener entre 10 y 100 caracteres.");

        var speciality = await _service.Add(request);
        return Ok(speciality);
    }

    [HttpPut("specialities/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SpecialityModel.Request request)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 100)
            return BadRequest("El campo 'name' es obligatorio y debe tener entre 3 y 100 caracteres.");

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10 || request.Description.Length > 100)
            return BadRequest("El campo 'description' es obligatorio y debe tener entre 10 y 100 caracteres.");

        var existing = await _service.GetById(id);
        if (existing == null)
            return NotFound($"La especialidad con id {id} no existe.");

        // Actualizar
        await _service.Update(id, request);

        return Ok(existing);

    }

    [HttpDelete("specialities/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _service.Delete(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }

    }
}
   

