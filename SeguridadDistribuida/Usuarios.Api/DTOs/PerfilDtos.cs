using System.ComponentModel.DataAnnotations;

namespace Usuarios.Api.DTOs
{

    // Lo envía Auth.Api durante el registro
    public class CrearPerfilRequest
    {
        [Required, StringLength(450)]
        public string UsuarioId { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Apellido { get; set; }

        [Required, EmailAddress, StringLength(256)]
        public string Correo { get; set; } = string.Empty;
    }

    // Datos que el usuario SÍ puede editar (no incluye correo, UsuarioId ni roles)
    public class ActualizarPerfilRequest
    {
        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Apellido { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(250)]
        public string? Direccion { get; set; }

        [StringLength(100)]
        public string? Ciudad { get; set; }
    }
}
