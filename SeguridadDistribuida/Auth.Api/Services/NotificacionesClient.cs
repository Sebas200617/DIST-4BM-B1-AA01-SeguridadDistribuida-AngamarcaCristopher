namespace Auth.Api.Services
{
    public class NotificacionesClient
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<NotificacionesClient> _logger;

        public NotificacionesClient(IHttpClientFactory factory, ILogger<NotificacionesClient> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task<bool> EnviarAsync(string? usuarioId, string destinatario, string asunto, string cuerpoHtml, string tipo)
        {
            var http = _factory.CreateClient("NotificacionesApi");
            try
            {
                using var resp = await http.PostAsJsonAsync("api/correos/enviar",
                    new { usuarioId, destinatario, asunto, cuerpoHtml, tipo });

                if (!resp.IsSuccessStatusCode)
                    _logger.LogWarning("Notificaciones.Api respondió HTTP {Codigo} (tipo {Tipo})", (int)resp.StatusCode, tipo);

                return resp.IsSuccessStatusCode;
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Timeout al llamar a Notificaciones.Api (tipo {Tipo})", tipo);
                return false;
            }
            catch (HttpRequestException)
            {
                _logger.LogError("Notificaciones.Api no disponible (tipo {Tipo})", tipo);
                return false;
            }
        }
    }
}
