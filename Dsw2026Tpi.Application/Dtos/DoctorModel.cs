using System.ComponentModel.DataAnnotations;

namespace Dsw2026Tpi.Application.Dtos;

public record DoctorModel
{
    public record Request([Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        string Name,

        [Required(ErrorMessage = "La matrícula es obligatoria.")]
        string LicenseNumber,

        [Required(ErrorMessage = "La especialidad es obligatoria.")]
        Guid SpecialityId
    );
    public record Response(Guid Id, string Name, string LicenseNumber, SpecialityDto? Speciality);
    public record SpecialityDto(Guid? SpecialityId, string? Name);

    public record AvailabilityResponse(string Day, string StartTime, string EndTime);
}
