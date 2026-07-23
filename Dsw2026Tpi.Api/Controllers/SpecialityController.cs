using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Resources;
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
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FILTER_INVALID, "name"),
                nameof(ErrorCodes.FILTER_INVALID)
            ).WithDetail("name", "filter_length_invalid");
        }
        
        var speciality = await _service.GetAll(pageSize, pageIndex, name);
        return Ok(speciality);
    }

    [HttpPost("specialities")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] SpecialityModel.Request request)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_REQUIRED, "name"),
                nameof(ErrorCodes.FIELD_REQUIRED)
            ).WithDetail("name", "required");
        }

        if (request.Name.Length < 3 || request.Name.Length > 100)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_LENGTH_INVALID, "name", 3, 100),
                nameof(ErrorCodes.FIELD_LENGTH_INVALID)
            ).WithDetail("name", "length_out_of_range");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_REQUIRED, "description"),
                nameof(ErrorCodes.FIELD_REQUIRED)
            ).WithDetail("description", "required");
        }

        if (request.Description.Length < 10 || request.Description.Length > 100)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_LENGTH_INVALID, "description", 10, 100),
                nameof(ErrorCodes.FIELD_LENGTH_INVALID)
            ).WithDetail("description", "length_out_of_range");
        }

        var speciality = await _service.Add(request);
        return Ok(speciality);
    }

    [HttpPut("specialities/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SpecialityModel.Request request)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_REQUIRED, "name"),
                nameof(ErrorCodes.FIELD_REQUIRED)
            ).WithDetail("name", "required");
        }

        if (request.Name.Length < 3 || request.Name.Length > 100)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_LENGTH_INVALID, "name", 3, 100),
                nameof(ErrorCodes.FIELD_LENGTH_INVALID)
            ).WithDetail("name", "length_out_of_range");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_REQUIRED, "description"),
                nameof(ErrorCodes.FIELD_REQUIRED)
            ).WithDetail("description", "required");
        }

        if (request.Description.Length < 10 || request.Description.Length > 100)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_LENGTH_INVALID, "description", 10, 100),
                nameof(ErrorCodes.FIELD_LENGTH_INVALID)
            ).WithDetail("description", "length_out_of_range");
        }

        var speciality = await _service.Update(id, request);
        return Ok(speciality);
    }

    [HttpDelete("specialities/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);
        return NoContent();
    }
}
