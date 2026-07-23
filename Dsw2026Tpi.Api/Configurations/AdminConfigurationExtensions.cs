using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Data.Identity;
using Microsoft.AspNetCore.Identity;

namespace Dsw2026Tpi.Api.Configurations
{
    public static class AdminSeederConfigurationExtensions
    {
        // Fijate que usamos WebApplication en vez de IServiceCollection
        // y lo hacemos "async Task" porque usamos "await" adentro.
        public static async Task UseAppAdminAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // 1. Aseguramos que existe el rol Administrador
                if (!await roleManager.RoleExistsAsync(Roles.Administrator))
                {
                    await roleManager.CreateAsync(new IdentityRole(Roles.Administrator));
                }

                // 2. Creamos al usuario Administrador si no existe
                var adminEmail = "admin@hospital.com";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin1234");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, Roles.Administrator);
                    }
                }
            }
        }
    }
}