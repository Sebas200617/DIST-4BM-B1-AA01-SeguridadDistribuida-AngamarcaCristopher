using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Notificaciones.Api.Data;
using Notificaciones.Api.DTOs;
using Notificaciones.Api.Model;


namespace Notificaciones.Api.Services
{
    public class CorreoService
    {
        private readonly SmtpOptions _smtp;
        private readonly NotificacionesDbContext _db;
        private readonly ILogger<CorreoService> _logger;

        public CorreoService(IOptions<SmtpOptions> smtp, NotificacionesDbContext db, ILogger<CorreoService> logger)
        {
            _smtp = smtp.Value;
            _db = db;
            _logger = logger;
        }

        public async Task<bool> EnviarAsync(EnviarCorreoRequest req, CancellationToken ct = default)
        {
            var registro = new EnvioCorreo
            {
                UsuarioId = req.UsuarioId,
                Destinatario = req.Destinatario,
                Asunto = req.Asunto,
                Tipo = req.Tipo,
                FechaEnvio = DateTime.UtcNow
            };

            try
            {
                var mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress(_smtp.NombreRemitente, _smtp.Usuario));
                mensaje.To.Add(MailboxAddress.Parse(req.Destinatario));
                mensaje.Subject = req.Asunto;
                mensaje.Body = new BodyBuilder { HtmlBody = req.CuerpoHtml }.ToMessageBody();

                using var cliente = new SmtpClient();
                await cliente.ConnectAsync(_smtp.Host, _smtp.Port, SecureSocketOptions.StartTls, ct);
                await cliente.AuthenticateAsync(_smtp.Usuario, _smtp.Clave, ct);
                await cliente.SendAsync(mensaje, cancellationToken: ct);
                await cliente.DisconnectAsync(true, ct);

                registro.Estado = "Enviado";
            }
            catch (Exception ex)
            {
                // Solo se guarda el tipo de error, nunca el cuerpo ni el enlace/token
                registro.Estado = "Fallido";
                registro.MensajeError = ex.GetType().Name;
                _logger.LogError("Fallo al enviar correo tipo {Tipo}: {Error}", req.Tipo, ex.GetType().Name);
            }

            // El cuerpo del correo NO se guarda en la base de datos
            _db.EnviosCorreo.Add(registro);
            await _db.SaveChangesAsync(CancellationToken.None);

            return registro.Estado == "Enviado";
        }
    }
}
