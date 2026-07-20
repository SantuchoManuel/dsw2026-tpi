using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAuthenticationService
{
    Task<RegisterModel.Response> Register(RegisterModel.Request request);
    Task<LoginAdminModel.Response> LoginAdmin(LoginAdminModel.Request request);
    Task<LoginPatientModel.Response> LoginPatient(LoginPatientModel.Request request);
}
