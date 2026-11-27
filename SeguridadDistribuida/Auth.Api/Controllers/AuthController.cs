using Auth.Api.DTOs;
using Auth.Api.Model;
using Auth.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly JwtService _jwt;
        private readonly UsuariosClient _usuarios;
        private readonly NotificacionesClient _notificaciones;

        public AuthController(UserManager<ApplicationUser> users, JwtService jwt,
            UsuariosClient usuarios, NotificacionesClient notificaciones)
        {
            _users = users;
            _jwt = jwt;
            _usuarios = usuarios;
            _notificaciones = notificaciones;
        }

        // POST api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Registrar(RegistroRequest req)
        {
            var correo = req.Correo.Trim();

            if (await _users.FindByEmailAsync(correo) is not null)
                return Conflict(new { mensaje = "El correo ya está registrado." });

            // 1) Cuenta en estado Pendiente (no puede iniciar sesión todavía)
            var user = new ApplicationUser
            {
                UserName = correo,
                Email = correo,
                EmailConfirmed = false,
                Estado = "Pendiente"
            };

            var crear = await _users.CreateAsync(user, req.Password);
            if (!crear.Succeeded)
                return BadRequest(new { errores = crear.Errors.Select(e => e.Description) });

            await _users.AddToRoleAsync(user, "Cliente");

            // 2) Crear el perfil en Usuarios.Api
            var perfilOk = await _usuarios.CrearPerfilAsync(user.Id, req.Nombre, req.Apellido, correo);
            if (!perfilOk)
            {
                // Compensación: se elimina la cuenta (y el perfil por si llegó a crearse)
                await _usuarios.EliminarPerfilAsync(user.Id);
                await _users.DeleteAsync(user);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { mensaje = "No fue posible completar el registro. Intente más tarde." });
            }

            // 3) Activar la cuenta solo si todo salió bien
            user.Estado = "Activa";
            var activar = await _users.UpdateAsync(user);
            if (!activar.Succeeded)
            {
                await _usuarios.EliminarPerfilAsync(user.Id);
                await _users.DeleteAsync(user);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { mensaje = "No fue posible completar el registro." });
            }

            // 4) Correo de bienvenida (si falla, el registro sigue siendo válido)
            await _notificaciones.EnviarAsync(user.Id, correo, "Bienvenido al sistema",
                $"<p>Hola {System.Net.WebUtility.HtmlEncode(req.Nombre)}, tu cuenta fue creada correctamente.</p>",
                "Bienvenida");

            return StatusCode(StatusCodes.Status201Created,
                new { mensaje = "Registro exitoso.", usuarioId = user.Id });
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            var generico = new { mensaje = "Credenciales inválidas." };

            var user = await _users.FindByEmailAsync(req.Correo.Trim());
            if (user is null) return Unauthorized(generico);

            if (await _users.IsLockedOutAsync(user))
                return Unauthorized(new { mensaje = "Cuenta bloqueada temporalmente. Intente más tarde." });

            if (!await _users.CheckPasswordAsync(user, req.Password))
            {
                await _users.AccessFailedAsync(user); // cuenta el intento y bloquea al llegar al máximo
                return Unauthorized(generico);
            }

            if (user.Estado != "Activa")
                return Unauthorized(generico);

            await _users.ResetAccessFailedCountAsync(user);

            var roles = await _users.GetRolesAsync(user);
            var (token, expira) = _jwt.Generar(user, roles);

            return Ok(new { token, expira, roles });
        }
    }
}
