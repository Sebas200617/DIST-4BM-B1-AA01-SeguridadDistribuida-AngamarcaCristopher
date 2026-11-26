namespace Notificaciones.Api.Services
{
    public class SmtpOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Usuario { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public string NombreRemitente { get; set; } = "Notificaciones";
    }
}
