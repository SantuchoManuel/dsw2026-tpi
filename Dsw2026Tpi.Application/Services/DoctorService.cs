using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{

    private readonly IPersistence _persistence;

    public DoctorService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        // Filtramos usando tu propiedad IsActive
        var doctors = await _persistence.Paginate<Doctor, string>(
            pageSize,
            pageIndex,
            d => (string.IsNullOrWhiteSpace(name) || d.Name.Contains(name)) && d.IsActive,
            x => x.Name,
            nameof(Doctor.Speciality));

        return doctors.Map(d => new DoctorModel.Response(
            d.Id,
            d.Name,
            d.LicenseNumber,
            new DoctorModel.SpecialityDto(d.Speciality?.Id, d.Speciality?.Name)));
    }

    public async Task<DoctorModel.Response> Create(DoctorModel.Request request)
    {
        var specialty = await _persistence.First<Speciality>(s => s.Id == request.SpecialityId);//
        if (specialty == null) throw new InvalidOperationException("La especialidad seleccionada no existe.");

        // Usamos exactamente tu constructor
        var doctor = new Doctor(request.Name, request.LicenseNumber, specialty);

        await _persistence.Add(doctor);

        return await GetByIdInternal(doctor.Id);
    }

    public async Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request request)
    {
        var doctor = await _persistence.First<Doctor>(d => d.Id == id && d.IsActive);
        if (doctor == null) throw new KeyNotFoundException("El médico no existe.");

        var specialty = await _persistence.First<Speciality>(s => s.Id == request.SpecialityId);
        if (specialty == null) throw new InvalidOperationException("La especialidad seleccionada no existe.");

        // ATENCIÓN ACÁ (Leé la nota abajo sobre las líneas rojas)
        doctor.Name = request.Name;
        doctor.LicenseNumber = request.LicenseNumber;
        doctor.SpecialityId = request.SpecialityId;

        await _persistence.Update(doctor);

        return await GetByIdInternal(doctor.Id);
    }

    public async Task Delete(Guid id)
    {
        var doctor = await _persistence.First<Doctor>(d => d.Id == id && d.IsActive);
        if (doctor == null) throw new KeyNotFoundException("El médico no existe.");

        // Usamos tu método para desactivarlo
        doctor.Deactivate();
        await _persistence.Update(doctor);
    }

    public async Task<List<DoctorModel.AvailabilityResponse>> GetAvailabilities(Guid id)
    {
        var doctor = await _persistence.First<Doctor>(d => d.Id == id && d.IsActive);
        if (doctor == null) throw new KeyNotFoundException("El médico no existe.");

        // Asegurate de que tu entidad de disponibilidad use IsActive también
        var availabilities = await _persistence.GetFiltered<Disponibilidad>(a => a.DoctorId == id );

        return availabilities?.Select(a => new DoctorModel.AvailabilityResponse(
            a.DayOfWeek.ToString(),
            a.StartTime.ToString(@"hh\:mm"),
            a.EndTime.ToString(@"hh\:mm")
        )).ToList() ?? new List<DoctorModel.AvailabilityResponse>();
    }

    private async Task<DoctorModel.Response> GetByIdInternal(Guid id)
    {
        var doctor = await _persistence.First<Doctor>(
            d => d.Id == id,
            nameof(Doctor.Speciality)
        );

        return new DoctorModel.Response(
            doctor.Id,
            doctor.Name,
            doctor.LicenseNumber,
            new DoctorModel.SpecialityDto(doctor.Speciality?.Id, doctor.Speciality?.Name)
        );
    }
}
