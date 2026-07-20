using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IDoctorService
{
    Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null);
    Task<List<DoctorModel.AvailabilityResponse>> GetAvailabilities(Guid id);
    Task<DoctorModel.Response> Create(DoctorModel.Request request);
    Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request request);
    Task Delete(Guid id);
}
