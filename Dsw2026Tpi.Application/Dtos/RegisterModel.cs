using System.ComponentModel.DataAnnotations;

namespace Dsw2026Tpi.Application.Dtos;

public record RegisterModel
{
    public record Request(string Email, string Password);
    public record Response(string Email);
}