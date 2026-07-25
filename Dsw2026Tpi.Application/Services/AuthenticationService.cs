using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Helpers;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Data.Identity;
using Dsw2026Tpi.Domain.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dsw2026Tpi.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISignInService _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IPersistence _persistence;

    public AuthenticationService(UserManager<ApplicationUser> userManager,
        ISignInService signInManager,
        RoleManager<IdentityRole> roleManager,
        JwtService jwtService,
        ILogger<AuthenticationService> logger,
        IPersistence persistence)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _logger = logger;
        _persistence = persistence;
    }

    public async Task<LoginAdminModel.Response> LoginAdmin(LoginAdminModel.Request request)
    {
        ValidateAdminLogin(request);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Intento de login con email inexistente: {Email}", request.Email);
            throw new AuthenticationException();
        }

        var result = await _signInManager.CheckPassword(user, request.Password);

        if (!result)
        {
            _logger.LogWarning("Intento de login fallido para: {Email}", request.Email);
            throw new AuthenticationException();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? Roles.Administrator;

        var token = _jwtService.GenerateToken(user.UserName!, role);

        return new LoginAdminModel.Response(token, role);
    }

    public async Task<LoginPatientModel.Response> LoginPatient(LoginPatientModel.Request request)
    {
        ValidatePatientLogin(request);

        var patient = await _persistence.First<Paciente>(p => p.Dni == request.Dni);

        if (patient == null)
        {
            patient = new Paciente(
                request.Dni,
                request.Email,
                "Sin Nombre",
                "Sin Celular"
            );

            await _persistence.Add(patient);
            _logger.LogInformation("Paciente creado automáticamente. DNI: {Dni}", request.Dni);
        }

        var token = _jwtService.GenerateToken(request.Email, Roles.Patient);

        return new LoginPatientModel.Response(token, Roles.Patient);
    }

    public async Task<RegisterModel.Response> Register(RegisterModel.Request request)
    {
        ValidateRegister(request);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var conflictException = new ConflictException(
                nameof(ErrorCodes.REGISTER_USER_CONFLICT),
                ErrorCodes.REGISTER_USER_CONFLICT);

            foreach (var error in result.Errors)
            {
                conflictException.WithDetail(error.Code, error.Description);
            }
            throw conflictException;
        }

        await _userManager.AddToRoleAsync(user, Roles.Administrator);
        _logger.LogInformation("Usuario registrado: {Email}", request.Email);

        return new RegisterModel.Response(request.Email);
    }

    private static void ValidateAdminLogin(LoginAdminModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.IsEmailValid())
        {
            throw new ValidationException("Formato de correo inválido", "FIELD_INVALID").WithDetail("Email", "Formato inválido");
        }
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("La contraseña es requerida", "FIELD_REQUIRED").WithDetail("Password", "Requerida");
        }
    }

    private static void ValidatePatientLogin(LoginPatientModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.IsEmailValid())
        {
            throw new ValidationException("Formato de correo inválido", "FIELD_INVALID").WithDetail("Email", "Formato inválido");
        }

        var dniString = request.Dni.ToString();
        if (dniString.Length < 7 || dniString.Length > 8)
        {
            throw new ValidationException("El DNI debe tener 7 u 8 dígitos", "FIELD_INVALID").WithDetail("Dni", "Longitud inválida");
        }
    }

    private static void ValidateRegister(RegisterModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.IsEmailValid())
        {
            throw new ValidationException("Formato de correo inválido", "FIELD_INVALID").WithDetail("Email", "Formato inválido");
        }
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            throw new ValidationException("La contraseña debe tener mínimo 8 caracteres", "FIELD_INVALID").WithDetail("Password", "Mínimo 8 caracteres requeridos");
        }
    }
}