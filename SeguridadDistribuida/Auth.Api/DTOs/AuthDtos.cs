using System.ComponentModel.DataAnnotations;

namespace Auth.Api.DTOs
{
    public class RegistroRequest
    {
        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Apellido { get; set; }

        [Required, EmailAddress, StringLength(256)]
        public string Correo { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class OlvideRequest
    {
        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;
    }

    public class RestablecerRequest
    {
        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        public string NuevaPassword { get; set; } = string.Empty;

        [Required, Compare(nameof(NuevaPassword))]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }

    public class CambiarPasswordRequest
    {
        [Required]
        public string PasswordActual { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        public string NuevaPassword { get; set; } = string.Empty;

        [Required, Compare(nameof(NuevaPassword))]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }

    public class CambiarRolRequest
    {
        [Required]
        public string Rol { get; set; } = string.Empty; // Cliente o Administrador
    }

    public class CambiarEstadoRequest
    {
        [Required]
        public string Estado { get; set; } = string.Empty; // Activa o Deshabilitada
    }

    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}
