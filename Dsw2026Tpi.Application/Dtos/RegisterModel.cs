using System.ComponentModel.DataAnnotations;

namespace Dsw2026Tpi.Application.Dtos;

public record RegisterModel
{
    public record Request(
    [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        [StringLength(256, ErrorMessage = "El email no puede superar 256 caracteres.")]
        string Email,
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
        string Password
);
    public record Response(string Email);
}
