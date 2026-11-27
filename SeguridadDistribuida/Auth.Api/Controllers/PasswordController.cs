using Auth.Api.DTOs;
using Auth.Api.Model;
using Auth.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;

namespace Auth.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class PasswordController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly NotificacionesClient _notificaciones;
        private readonly IConfiguration _config;

        public PasswordController(UserManager<ApplicationUser> users,
            NotificacionesClient notificaciones, IConfiguration config)
        {
            _users = users;
            _notificaciones = notificaciones;
            _config = config;
        }

        // POST api/auth/forgot-password  (respuesta SIEMPRE genérica)
        [HttpPost("forgot-password")]
        public async Task<IActionResult> Olvide(OlvideRequest req)
        {
            var generica = Ok(new { mensaje = "Si el correo está registrado, recibirá un enlace de restablecimiento." });

            var user = await _users.FindByEmailAsync(req.Correo.Trim());
            if (user is null || user.Estado != "Activa") return generica;

            var token = await _users.GeneratePasswordResetTokenAsync(user);
            var tokenCodificado = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var baseUrl = _config["Servicios:UrlBaseEnlaceRecuperacion"]!;
            var enlace = $"{baseUrl}?correo={Uri.EscapeDataString(user.Email!)}&token={tokenCodificado}";

            var html = $"<p>Recibimos una solicitud para restablecer tu contraseña.</p>" +
                       $"<p><a href=\"{System.Net.WebUtility.HtmlEncode(enlace)}\">Restablecer contraseña</a></p>" +
                       $"<p>El enlace vence en 30 minutos. Si no fuiste tú, ignora este mensaje.</p>";

            // Si falla, solo se registra el fallo (sin token) y la respuesta sigue siendo genérica
            await _notificaciones.EnviarAsync(user.Id, user.Email!, "Restablecer contraseña", html, "Recuperacion");

            return generica;
        }

        // POST api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> Restablecer(RestablecerRequest req)
        {
            var rechazo = BadRequest(new { mensaje = "El enlace no es válido o ha vencido." });

            var user = await _users.FindByEmailAsync(req.Correo.Trim());
            if (user is null) return rechazo;

            string token;
            try
            {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(req.Token));
            }
            catch (FormatException)
            {
                return rechazo; // token alterado
            }

            var resultado = await _users.ResetPasswordAsync(user, token, req.NuevaPassword);
            if (!resultado.Succeeded)
            {
                // Token vencido/reutilizado/alterado, o contraseña débil
                var debil = resultado.Errors.Any(e => e.Code.StartsWith("Password"));
                return debil
                    ? BadRequest(new { errores = resultado.Errors.Select(e => e.Description) })
                    : rechazo;
            }

            await _users.ResetAccessFailedCountAsync(user);
            await _users.SetLockoutEndDateAsync(user, null);

            return Ok(new { mensaje = "Contraseña actualizada." });
        }

        // POST api/auth/change-password (usuario autenticado)
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> Cambiar(CambiarPasswordRequest req)
        {
            var user = await _users.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (user is null) return Unauthorized();

            var resultado = await _users.ChangePasswordAsync(user, req.PasswordActual, req.NuevaPassword);
            if (!resultado.Succeeded)
                return BadRequest(new { mensaje = "No se pudo cambiar la contraseña. Verifique los datos." });

            return Ok(new { mensaje = "Contraseña cambiada." });
        }
    }
}
