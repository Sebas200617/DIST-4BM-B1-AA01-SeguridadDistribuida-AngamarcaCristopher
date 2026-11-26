namespace Usuarios.Api.Model
{
    public class HistorialCambioPerfil
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public string Campo { get; set; } = string.Empty;
        public string? ValorAnterior { get; set; }
        public string? ValorNuevo { get; set; }
        public DateTime FechaCambio { get; set; }
    }
}
