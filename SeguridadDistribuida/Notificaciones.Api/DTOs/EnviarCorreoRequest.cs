using System.ComponentModel.DataAnnotations;

namespace Notificaciones.Api.DTOs
{
    public class EnviarCorreoRequest
    {
        public string? UsuarioId { get; set; }

        [Required, EmailAddress, StringLength(256)]
        public string Destinatario { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Asunto { get; set; } = string.Empty;

        // Contenido HTML del mensaje (puede incluir el enlace de recuperación)
        [Required]
        public string CuerpoHtml { get; set; } = string.Empty;

        // Bienvenida, Recuperacion, AvisoPerfil
        [Required, StringLength(50)]
        public string Tipo { get; set; } = string.Empty;
    }
}
