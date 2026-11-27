using Auth.Api.DTOs;
using Auth.Api.Model;
using Auth.Api.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class GoogleController : ControllerBase
    {
        private const string Proveedor = "Google";

        private readonly UserManager<ApplicationUser> _users;
        private readonly JwtService _jwt;
        private readonly UsuariosClient _usuarios;
        private readonly IConfiguration _config;

        public GoogleController(UserManager<ApplicationUser> users, JwtService jwt,
            UsuariosClient usuarios, IConfiguration config)
        {
            _users = users;
            _jwt = jwt;
            _usuarios = usuarios;
            _config = config;
        }

        // POST api/auth/google
        [HttpPost("google")]
        public async Task<IActionResult> Google(GoogleLoginRequest req)
        {
            var invalido = Unauthorized(new { mensaje = "No fue posible validar la identidad con Google." });

            // 1) Validar el ID token EN EL SERVIDOR (firma, issuer, audience y expiración)
            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _config["Google:ClientId"] }
                    });
            }
            catch (InvalidJwtException)
            {
                return invalido;
            }

            if (!payload.EmailVerified || string.IsNullOrEmpty(payload.Email))
                return invalido;

            // 2) Buscar la identidad externa por el identificador estable (sub)
            var user = await _users.FindByLoginAsync(Proveedor, payload.Subject);

            if (user is null)
            {
                // No se vincula automáticamente una cuenta local preexistente
                if (await _users.FindByEmailAsync(payload.Email) is not null)
                    return Conflict(new
                    {
                        mensaje = "Ya existe una cuenta con ese correo. Inicie sesión con su contraseña."
                    });

                // 3) Cuenta nueva: siempre rol Cliente
                user = new ApplicationUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    EmailConfirmed = true,
                    Estado = "Pendiente"
                };

                var crear = await _users.CreateAsync(user);
                if (!crear.Succeeded) return StatusCode(500, new { mensaje = "No fue posible completar el acceso." });

                await _users.AddToRoleAsync(user, "Cliente");
                await _users.AddLoginAsync(user, new UserLoginInfo(Proveedor, payload.Subject, Proveedor));

                var nombre = payload.GivenName ?? payload.Name ?? payload.Email;
                var perfilOk = await _usuarios.CrearPerfilAsync(user.Id, nombre, payload.FamilyName, payload.Email);
                if (!perfilOk)
                {
                    // Compensación: no queda una cuenta incompleta
                    await _usuarios.EliminarPerfilAsync(user.Id);
                    await _users.DeleteAsync(user);
                    return StatusCode(503, new { mensaje = "No fue posible completar el acceso. Intente más tarde." });
                }

                user.Estado = "Activa";
                await _users.UpdateAsync(user);
            }

            if (user.Estado != "Activa" || await _users.IsLockedOutAsync(user))
                return invalido;

            // 4) JWT propio de la aplicación (el ID token de Google no se reutiliza)
            var roles = await _users.GetRolesAsync(user);
            var (token, expira) = _jwt.Generar(user, roles);

            return Ok(new { token, expira, roles });
        }
    }
}
