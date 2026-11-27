using Auth.Api.Model;
using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {
            var roles = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var config = sp.GetRequiredService<IConfiguration>();
            var logger = sp.GetRequiredService<ILogger<AuthDbContext>>();

            foreach (var rol in new[] { "Cliente", "Administrador" })
                if (!await roles.RoleExistsAsync(rol))
                    await roles.CreateAsync(new IdentityRole(rol));

            var email = config["AdminInicial:Email"];
            var password = config["AdminInicial:Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("AdminInicial no configurado en User Secrets: no se crea administrador.");
                return;
            }

            if (await users.FindByEmailAsync(email) is not null) return;

            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Estado = "Activa"
            };

            var resultado = await users.CreateAsync(admin, password);
            if (resultado.Succeeded)
                await users.AddToRoleAsync(admin, "Administrador");
            else
                logger.LogError("No se pudo crear el administrador inicial: {Errores}",
                    string.Join("; ", resultado.Errors.Select(e => e.Code)));
        }
    }
}
