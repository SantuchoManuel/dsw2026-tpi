using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
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

    public async Task<SpecialityModel.Response> Update(Guid id, SpecialityModel.Request request)
    {
        var speciality = await _persistence.First<Speciality>(s => s.Id == id);
        if (speciality == null) throw new InvalidOperationException("La especialidad seleccionada no existe.");

        //aqui se deberia hacer una mini validacion para cambiarlo si fuera diferente y dejarlo de ser iguales o asi
        speciality.Name = request.Name;
        speciality.Description = request.Description;

        await _persistence.Update(speciality);
        //Esto Ya anda, hay q revisar si puedo hacer q la respuesta envie lo nuevo cargado y no lo viejo --- Ademas cambie speciality y lo puse en set y no en init, para poder cambiarlo y que se guarde en la base de datos
        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }

}

