using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("specialities")]
//[Authorize(Policy = Policies.AdminPolicy)]
public class SpecialityController : AppController
{
    private readonly ISpecialityService _service;

    public SpecialityController(ISpecialityService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize =10 , [FromQuery] int pageIndex = 1, [FromQuery] string? name = null)
    {
        var speciality  = await _service.GetAll(pageSize, pageIndex, name);
        return Ok(speciality);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] SpecialityModel.Request request)
    {
        var speciality = await _service.Add(request);
        return Ok(speciality);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SpecialityModel.Request request)
    {
       var speciality = await _service.GetById(id);
       await _service.Update(id, request);
       return Ok(speciality);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var speciality = await _service.GetById(id);
        await _service.Delete(new SpecialityModel.Request(speciality.Name, speciality.Description));
        return Ok(speciality);  
    }
}

