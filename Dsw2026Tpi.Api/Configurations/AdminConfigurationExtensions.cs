using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Data.Identity;
using Microsoft.AspNetCore.Identity;

namespace Dsw2026Tpi.Api.Configurations
{
    public static class AdminSeederConfigurationExtensions
    {
        public static async Task UseAppAdminAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                if (!await roleManager.RoleExistsAsync(Roles.Administrator))
                {
                    await roleManager.CreateAsync(new IdentityRole(Roles.Administrator));
                }

                var adminEmail = app.Configuration["AdminSettings:Email"];
                var adminPassword = app.Configuration["AdminSettings:Password"];


                if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
                {
                    throw new ValidationException("Faltan las credenciales del Administrador en el appsettings.json", "AUTHENTICATION_FAILED");
                }

                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, Roles.Administrator);
                    }
                }
            }
        }
    }
}