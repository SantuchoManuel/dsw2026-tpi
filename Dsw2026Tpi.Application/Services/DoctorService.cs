using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
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
        var doctors = await _persistence.Paginate<Doctor, string>(
            pageSize,
            pageIndex,
            d => (string.IsNullOrWhiteSpace(name) || d.Name.Contains(name)) && d.IsActive && !d.Deleted,
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
        ValidateRequest(request);
        var specialty = await _persistence.First<Speciality>(s => s.Id == request.SpecialityId);//
        if (specialty == null) throw new EntityNotFoundException("Speciality").WithDetail("Speciality", "No Encontrado");


        var doctor = new Doctor(request.Name, request.LicenseNumber, specialty);

        await _persistence.Add(doctor);

        return await GetByIdInternal(doctor.Id);
    }

    public async Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request request)
    {
        ValidateRequest(request);
        var doctor = await _persistence.First<Doctor>(d => d.Id == id && d.IsActive && !d.Deleted);
        if (doctor == null) throw new EntityNotFoundException("Doctor").WithDetail(" Doctor", "No Encontrado");

        var specialty = await _persistence.First<Speciality>(s => s.Id == request.SpecialityId);
        if (specialty == null) throw new EntityNotFoundException("Speciality").WithDetail("speciality", "No Encontrado");


        doctor.Name = request.Name;
        doctor.LicenseNumber = request.LicenseNumber;
        doctor.SpecialityId = request.SpecialityId;

        await _persistence.Update(doctor);

        return await GetByIdInternal(doctor.Id);
    }

    public async Task Delete(Guid id)
    {
        var doctor = await _persistence.First<Doctor>(d => d.Id == id && !d.Deleted);
        if (doctor == null) throw new EntityNotFoundException("Doctor").WithDetail(" Doctor", "No Encontrado");

        doctor.MarkAsDeleted();
        await _persistence.Update(doctor);
    }

    public async Task<List<DoctorModel.AvailabilityResponse>> GetAvailabilities(Guid id)
    {
        var doctor = await _persistence.First<Doctor>(d => d.Id == id && d.IsActive && !d.Deleted);
        if (doctor == null) throw new EntityNotFoundException("Doctor").WithDetail(" Doctor", "No Encontrado");

        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        var availabilities = await _persistence.GetFiltered<Availability>(
            a => a.DoctorId == id && a.Month == currentMonth && a.Year == currentYear
        );

        return availabilities?.Select(a => new DoctorModel.AvailabilityResponse(
            a.Id,
            a.DayOfWeek.ToString(),
            a.StartTime.ToString("HH:mm"),
            a.EndTime.ToString("HH:mm")
        )).ToList() ?? new List<DoctorModel.AvailabilityResponse>();
    }

    public async Task<List<DoctorModel.SlotResponse>> GetSlots(Guid id, DateOnly? date = null)
    {
        var doctor = await _persistence.First<Doctor>(d => d.Id == id && d.IsActive && !d.Deleted);
        if (doctor == null) throw new EntityNotFoundException("Médico").WithDetail("DoctorId", "No Encontrado");
        // Obtenemos todos los turnos del doctor a través de su disponibilidad
        var turnos = await _persistence.GetFiltered<AvailabilitySlot>(
            t => t.Availability.DoctorId == id
                 && (!date.HasValue || t.Date == date.Value),
            nameof(AvailabilitySlot.Availability)
        );
        return turnos?
            .OrderBy(t => t.Date)
            .ThenBy(t => t.StartTime)
            .Select(t => new DoctorModel.SlotResponse(
                t.Id,
                t.Date,
                t.StartTime.ToString("HH:mm"),
                t.EndTime.ToString("HH:mm"),
                t.Status.ToString()
            ))
            .ToList() ?? new List<DoctorModel.SlotResponse>();
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
    private static void ValidateRequest(DoctorModel.Request request)
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

        if (string.IsNullOrWhiteSpace(request.LicenseNumber))
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_REQUIRED, "licenseNumber"),
                nameof(ErrorCodes.FIELD_REQUIRED)
            ).WithDetail("licenseNumber", "Es requerido");
        }

        if (request.LicenseNumber.Length < 3 || request.LicenseNumber.Length > 50)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_LENGTH_INVALID, "licenseNumber", 3, 50),
                nameof(ErrorCodes.FIELD_LENGTH_INVALID)
            ).WithDetail("licenseNumber", "La longitud es inválida");
        }

        if (request.SpecialityId == Guid.Empty)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_REQUIRED, "specialityId"),
                nameof(ErrorCodes.FIELD_REQUIRED)
            ).WithDetail("specialityId", "Es requerido");
        }
    }
}
