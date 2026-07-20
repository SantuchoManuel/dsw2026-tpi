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
        if (!request.Email.IsEmailValid()) throw new AuthenticationException();
        var user = await _userManager.FindByEmailAsync(request.Email) ?? throw new AuthenticationException();
        var result = await _signInManager.CheckPassword(user, request.Password);

        if (!result)
        {
            _logger.LogError("Intento de login fallido para: {Email}", request.Email);
            throw new AuthenticationException();
        }

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

        var token  = _jwtService.GenerateToken(user.UserName!, role);

        return new LoginAdminModel.Response(
            token,
            role
        );
    }

    public async Task<LoginPatientModel.Response> LoginPatient(LoginPatientModel.Request request)
    {
        if (!request.Email.IsEmailValid())
            throw new AuthenticationException();
        if (request.Dni < 1000000 || request.Dni > 99999999)
            throw new AuthenticationException();
        var patient = await _persistence.First<Paciente>(p => p.Dni == request.Dni);
        if (patient == null)
        {
            patient = new Paciente(
                (int)request.Dni,
                request.Email
            );

            await _persistence.Add(patient);

            _logger.LogInformation("Paciente creado automáticamente. DNI: {Dni}", request.Dni);
        }
        var token = _jwtService.GenerateToken(
            request.Email,
            Roles.Patient
        );
        return new LoginPatientModel.Response(
            token,
            Roles.Patient
        );
    }
    public async Task<RegisterModel.Response> Register(RegisterModel.Request request)
    {
        if (!request.Email.IsEmailValid()) throw new ValidationException(ErrorCodes.REGISTER_USER_INVALID,
            nameof(ErrorCodes.REGISTER_USER_INVALID));

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded) throw new ConflictException(nameof(ErrorCodes.REGISTER_USER_CONFLICT),
            ErrorCodes.REGISTER_USER_CONFLICT)
                .WithDetail(result.Errors.Select(e => (e.Code, e.Description)));
       
        _ = await _userManager.AddToRoleAsync(user, Roles.Administrator);

        _logger.LogInformation("Usuario registrado: {Email}", request.Email);

        return new RegisterModel.Response(request.Email);
    }
}
