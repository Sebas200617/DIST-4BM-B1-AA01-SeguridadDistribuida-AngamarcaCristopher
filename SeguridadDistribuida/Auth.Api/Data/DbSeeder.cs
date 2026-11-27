using Auth.Api.Model;
using Auth.Api.Services;
using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {
            var roles = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var usuarios = sp.GetRequiredService<UsuariosClient>();
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

            var admin = await users.FindByEmailAsync(email);

            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    Estado = "Activa"
                };

                var resultado = await users.CreateAsync(admin, password);
                if (!resultado.Succeeded)
                {
                    logger.LogError("No se pudo crear el administrador inicial: {Errores}",
                        string.Join("; ", resultado.Errors.Select(e => e.Code)));
                    return;
                }
                await users.AddToRoleAsync(admin, "Administrador");
            }

            // Crear el perfil si falta (si ya existe, Usuarios.Api responde 409 y no pasa nada)
            var ok = await usuarios.CrearPerfilAsync(admin.Id, "Administrador", null, email);
            if (!ok)
                logger.LogWarning("No se pudo crear/confirmar el perfil del administrador (¿Usuarios.Api está levantada?).");
        }
    }
}
