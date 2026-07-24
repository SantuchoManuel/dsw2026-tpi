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
        if (!string.IsNullOrEmpty(name) && (name.Length < 3 || name.Length > 100))
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_LENGTH_INVALID, "name", 3, 100),
                nameof(ErrorCodes.FIELD_LENGTH_INVALID)
            ).WithDetail("name", "La longitud es inválida");
        }

        var specialities = await _persistence.Paginate<Speciality, string>(
            pageSize,
            pageIndex,
            s => (string.IsNullOrWhiteSpace(name) || s.Name.Contains(name)) && s.IsDeleted == false,
            x => x.Name);

        return specialities.Map(s => new SpecialityModel.Response(s.Id, s.Name, s.Description));
    }

    public async Task<SpecialityModel.Response> Add(SpecialityModel.Request request)
    {
        ValidateRequest(request);

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
        ValidateRequest(request);

        var speciality = await _persistence.First<Speciality>(s => s.Id == id && s.IsDeleted == false);
        if (speciality == null) throw new EntityNotFoundException("Speciality").WithDetail("Speciality", "No Encontrada");

        speciality.Name = request.Name;
        speciality.Description = request.Description;

        await _persistence.Update(speciality);
        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }

    public async Task Delete(Guid id)
    {
        var speciality = await _persistence.First<Speciality>(s => s.Id == id && s.IsDeleted == false);
        if (speciality == null) throw new EntityNotFoundException("Speciality").WithDetail("Speciality", "Not Found");

        speciality.EstablecerDeleted();
        await _persistence.Update(speciality);
    }

    private static void ValidateRequest(SpecialityModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_REQUIRED, "name"),
                nameof(ErrorCodes.FIELD_REQUIRED)
            ).WithDetail("name", "Es requerido");
        }

        if (request.Name.Length < 3 || request.Name.Length > 100)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_LENGTH_INVALID, "name", 3, 100),
                nameof(ErrorCodes.FIELD_LENGTH_INVALID)
            ).WithDetail("name", "La longitud es inválida");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_REQUIRED, "description"),
                nameof(ErrorCodes.FIELD_REQUIRED)
            ).WithDetail("description", "Es requerido");
        }

        if (request.Description.Length < 10 || request.Description.Length > 100)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_LENGTH_INVALID, "description", 10, 100),
                nameof(ErrorCodes.FIELD_LENGTH_INVALID)
            ).WithDetail("description", "La longitud es inválida");
        }
    }
}