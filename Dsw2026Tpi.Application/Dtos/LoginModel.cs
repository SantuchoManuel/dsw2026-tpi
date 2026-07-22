using System.ComponentModel.DataAnnotations;

namespace Dsw2026Tpi.Application.Dtos;

public record LoginAdminModel
{
    public record Request(
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        string Email,
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        string Password
    );
    public record Response(string? Token, string? Role);
}
public record LoginPatientModel
{
    public record Request(
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        string Email,
        [Range(1000000, 99999999, ErrorMessage = "El DNI debe tener entre 7 y 8 dígitos.")]
        long Dni
    );
    public record Response(string? Token, string? Role);
}
