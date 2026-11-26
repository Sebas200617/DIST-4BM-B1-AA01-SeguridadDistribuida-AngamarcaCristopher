using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Cryptography;
using System.Text;

namespace Notificaciones.Api.Security
{
    public class ApiKeyFilter : IAsyncActionFilter
    {
        private readonly IConfiguration _config;

        public ApiKeyFilter(IConfiguration config) => _config = config;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var esperada = _config["ServiceApiKey"];

            if (string.IsNullOrEmpty(esperada) ||
                !context.HttpContext.Request.Headers.TryGetValue("X-Service-Key", out var recibida))
            {
                context.Result = new UnauthorizedResult(); // 401
                return;
            }

            var a = Encoding.UTF8.GetBytes(esperada);
            var b = Encoding.UTF8.GetBytes(recibida.ToString());

            if (!CryptographicOperations.FixedTimeEquals(a, b))
            {
                context.Result = new UnauthorizedResult(); // 401
                return;
            }

            await next();
        }
    }
}