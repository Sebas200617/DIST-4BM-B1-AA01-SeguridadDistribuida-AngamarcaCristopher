namespace Notificaciones.Api.Model
{
    public class EnvioCorreo
    {
        public int Id { get; set; }
        public string? UsuarioId { get; set; }
        public string Destinatario { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? MensajeError { get; set; }
        public DateTime FechaEnvio { get; set; }
    }
}