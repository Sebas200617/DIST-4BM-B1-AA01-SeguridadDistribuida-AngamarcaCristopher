namespace Auth.Api.Services
{
    public class UsuariosClient
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<UsuariosClient> _logger;

        public UsuariosClient(IHttpClientFactory factory, ILogger<UsuariosClient> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task<bool> CrearPerfilAsync(string usuarioId, string nombre, string? apellido, string correo)
        {
            var http = _factory.CreateClient("UsuariosApi");
            try
            {
                using var resp = await http.PostAsJsonAsync("api/perfiles",
                    new { usuarioId, nombre, apellido, correo });

                if (!resp.IsSuccessStatusCode)
                    _logger.LogWarning("Usuarios.Api respondió HTTP {Codigo} al crear perfil", (int)resp.StatusCode);

                return resp.IsSuccessStatusCode;
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Timeout del HttpClient al llamar a Usuarios.Api");
                return false;
            }
            catch (HttpRequestException)
            {
                _logger.LogError("Usuarios.Api no disponible");
                return false;
            }
        }

        // Compensación: borra el perfil si el registro falla
        public async Task EliminarPerfilAsync(string usuarioId)
        {
            var http = _factory.CreateClient("UsuariosApi");
            try
            {
                using var resp = await http.DeleteAsync($"api/perfiles/{usuarioId}");
            }
            catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException)
            {
                _logger.LogWarning("No se pudo compensar el perfil en Usuarios.Api");
            }
        }
    }
}
