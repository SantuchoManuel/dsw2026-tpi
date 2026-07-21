using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces;

public interface ISpecialityService
{
    Task<Pagination<SpecialityModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null);

    Task<SpecialityModel.Response> Add(SpecialityModel.Request request);
    Task<SpecialityModel.Response> Update(Guid id, SpecialityModel.Request request);
    Task<SpecialityModel.Response> Delete(SpecialityModel.Request request);
    Task<SpecialityModel.Response> GetById(Guid id);

}

