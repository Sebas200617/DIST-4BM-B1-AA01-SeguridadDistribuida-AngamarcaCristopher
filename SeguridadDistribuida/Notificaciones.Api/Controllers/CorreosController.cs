using Microsoft.AspNetCore.Mvc;
using Notificaciones.Api.DTOs;
using Notificaciones.Api.Security;
using Notificaciones.Api.Services;

namespace Notificaciones.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ApiKeyFilter))]
    public class CorreosController : ControllerBase
    {
        private readonly CorreoService _correo;

        public CorreosController(CorreoService correo) => _correo = correo;

        // POST api/correos/enviar
        [HttpPost("enviar")]
        public async Task<IActionResult> Enviar(EnviarCorreoRequest request, CancellationToken ct)
        {
            var ok = await _correo.EnviarAsync(request, ct);

            if (!ok)
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { mensaje = "No se pudo enviar el correo." });

            return Ok(new { mensaje = "Correo enviado." });
        }
    }
}