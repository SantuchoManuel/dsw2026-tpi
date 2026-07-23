using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Dsw2026Tpi.CrossCutting.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
       var specialities = await _persistence.Paginate<Speciality, string>(
           pageSize,
           pageIndex,
           s => (string.IsNullOrWhiteSpace(name) || s.Name.Contains(name)) && s.IsDeleted == false,
           x => x.Name);

       return specialities.Map(s => new SpecialityModel.Response(s.Id, s.Name, s.Description));
    }

    public async Task<SpecialityModel.Response> Add(SpecialityModel.Request request)
    {
        var speciality = new Speciality(request.Name, request.Description);
        await _persistence.Add(speciality);
        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }

    public async Task<SpecialityModel.Response?> GetById(Guid id)
    {
        var speciality = await _persistence.First<Speciality>(s => s.Id == id && s.IsDeleted == false);
        if (speciality == null) return null;
        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }

    public async Task<SpecialityModel.Response> Update(Guid id, SpecialityModel.Request request)
    {
        var speciality = await _persistence.First<Speciality>(s => s.Id == id && s.IsDeleted == false);
        if (speciality == null) throw new EntityNotFoundException("Speciality");

        speciality.Name = request.Name;
        speciality.Description = request.Description;

        await _persistence.Update(speciality);
        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }

    public async Task Delete(Guid id)
    {
        var speciality = await _persistence.First<Speciality>(s => s.Id == id && s.IsDeleted == false);
        if (speciality == null) throw new EntityNotFoundException("Speciality");

        speciality.EstablecerDeleted();
        await _persistence.Update(speciality);
    }

    private void ValidateRequest(SpecialityModel.Request request)
    {
        var details = new List<(string, string)>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            details.Add((nameof(request.Name), "required"));
        }
        else if (request.Name.Length < 3 || request.Name.Length > 100)
        {
            details.Add((nameof(request.Name), "invalid_length"));
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            details.Add((nameof(request.Description), "required"));
        }
        else if (request.Description.Length < 10 || request.Description.Length > 100)
        {
            details.Add((nameof(request.Description), "invalid_length"));
        }

        if (details.Any())
        {
            throw new ValidationException(ErrorCodes.VALIDATION_ERROR, nameof(ErrorCodes.VALIDATION_ERROR))
                .WithDetail(details.Select(d => (ToCamelCase(d.Item1), d.Item2)));
        }
    }

    private static string ToCamelCase(string s)
    {
        if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            return s;
        return char.ToLower(s[0]) + s.Substring(1);
    }
}
