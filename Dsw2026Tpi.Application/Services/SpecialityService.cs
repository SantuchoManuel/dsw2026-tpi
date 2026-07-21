using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class SpecialityService : ISpecialityService
{
    private readonly IPersistence _persistence;

    public SpecialityService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<SpecialityModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
       var specialities = await _persistence.Paginate<Speciality, string>(pageSize, pageIndex, s => string.IsNullOrWhiteSpace(name) ||
                                                   s.Name.Contains(name), x => x.Name);
       return specialities.Map(s => new SpecialityModel.Response(s.Id, s.Name, s.Description));
    }

    public async Task<SpecialityModel.Response> Add(SpecialityModel.Request request)
    {
        var speciality = new Speciality(request.Name, request.Description);
        await _persistence.Add(speciality);
        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }

    /*
    var _doctor = await (GetDoctor(id));
        _doctor.Deactivate();
    await _persistence.UpdateDoctor(_doctor);
    return   NoContent(); 
    */

    public async Task<SpecialityModel.Response> Delete(SpecialityModel.Request request)
    {
        var speciality = await _persistence.First<Speciality>(s => s.Name == request.Name);
        await _persistence.Delete(speciality);
        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }
    public async Task<SpecialityModel.Response> GetById(Guid id)
    {
        var speciality = await _persistence.First<Speciality>(s => s.Id == id);
        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }

    public async Task<SpecialityModel.Response> Update(SpecialityModel.Request request)
    {
        var speciality = await _persistence.First<Speciality>(s => s.Name == request.Name);
        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }

}

