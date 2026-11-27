using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Model
{
    public class ApplicationUser : IdentityUser
    {
        // Pendiente, Activa, Deshabilitada
        public string Estado { get; set; } = "Pendiente";
    }
}
