using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/doctors")]
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
        var result = await _doctorService.GetAvailabilities(id);
        return Ok(result);

    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminPolicy)]
    public async Task<IActionResult> Create([FromBody] DoctorModel.Request request)
    {
        var result = await _doctorService.Create(request);
        return Ok(result);

    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    public async Task<IActionResult> Update(Guid id, [FromBody] DoctorModel.Request request)
    {
        var result = await _doctorService.Update(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _doctorService.Delete(id);
        return Ok("ok");
    }

}
