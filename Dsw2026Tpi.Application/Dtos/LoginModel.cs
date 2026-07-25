using System.ComponentModel.DataAnnotations;

namespace Dsw2026Tpi.Application.Dtos;

public record LoginAdminModel
{
    public record Request(string Email, string Password);
    public record Response(string Token, string Role);
}
public record LoginPatientModel
{
    public record Request(string Email, int Dni);
    public record Response(string Token, string Role);
}
