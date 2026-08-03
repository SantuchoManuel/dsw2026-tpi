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

        if (patient != null)
        {
            if (patient.Email != request.Email)
            {
                throw new ValidationException(
                    string.Format(ErrorCodes.FIELD_INVALID, "Credenciales"),
                    nameof(ErrorCodes.FIELD_INVALID)
                ).WithDetail("Login", "El DNI ingresado ya está registrado con otro correo electrónico.");
            }
        }
        else
        {
            var emailInUse = await _persistence.First<Paciente>(p => p.Email == request.Email);

            if (emailInUse != null)
            {
                throw new ValidationException(
                    string.Format(ErrorCodes.FIELD_INVALID, "Credenciales"),
                    nameof(ErrorCodes.FIELD_INVALID)
                ).WithDetail("Login", "Este correo electrónico ya está asociado a otro DNI.");
            }

            patient = new Paciente(
                request.Dni,
                request.Email,
                "Sin Nombre",
                "Sin Celular"
            );
            await _persistence.Add(patient);
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
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_INVALID, "Email"),
                nameof(ErrorCodes.FIELD_INVALID)
            ).WithDetail("Email", "Formato de correo inválido");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_REQUIRED, "Password"),
                nameof(ErrorCodes.FIELD_REQUIRED)
            ).WithDetail("Password", "Es requerido");
        }
    }

    private static void ValidatePatientLogin(LoginPatientModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.IsEmailValid())
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_INVALID, "Email"),
                nameof(ErrorCodes.FIELD_INVALID)
            ).WithDetail("Email", "Formato de correo inválido");
        }

        var dniString = request.Dni.ToString();
        if (dniString.Length < 7 || dniString.Length > 8)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_LENGTH_INVALID, "Dni", 7, 8),
                nameof(ErrorCodes.FIELD_LENGTH_INVALID)
            ).WithDetail("Dni", "La longitud es inválida");
        }
    }

    private static void ValidateRegister(RegisterModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.IsEmailValid())
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_INVALID, "Email"),
                nameof(ErrorCodes.FIELD_INVALID)
            ).WithDetail("Email", "Formato de correo inválido");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_REQUIRED, "Password"),
                nameof(ErrorCodes.FIELD_REQUIRED)
            ).WithDetail("Password", "Es requerido");
        }

        if (request.Password.Length < 8)
        {
            throw new ValidationException(
                string.Format(ErrorCodes.FIELD_LENGTH_INVALID, "Password", 8, 100),
                nameof(ErrorCodes.FIELD_LENGTH_INVALID)
            ).WithDetail("Password", "La longitud es inválida");
        }
    }
}