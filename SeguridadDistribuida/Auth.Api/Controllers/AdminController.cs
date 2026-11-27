using Auth.Api.DTOs;
using Auth.Api.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Administrador")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;

        public AdminController(UserManager<ApplicationUser> users) => _users = users;

        // GET api/admin/usuarios
        [HttpGet("usuarios")]
        public async Task<IActionResult> Listar()
        {
            var lista = await _users.Users.AsNoTracking().ToListAsync();
            var resultado = new List<object>();

            foreach (var u in lista)
            {
                var roles = await _users.GetRolesAsync(u);
                resultado.Add(new { u.Id, u.Email, u.Estado, roles });
            }
            return Ok(resultado);
        }

        // PUT api/admin/usuarios/{id}/rol
        [HttpPut("usuarios/{id}/rol")]
        public async Task<IActionResult> CambiarRol(string id, CambiarRolRequest req)
        {
            if (req.Rol is not ("Cliente" or "Administrador"))
                return BadRequest(new { mensaje = "Rol inválido." });

            var user = await _users.FindByIdAsync(id);
            if (user is null) return NotFound();

            if (req.Rol != "Administrador"
                && await _users.IsInRoleAsync(user, "Administrador")
                && await EsUltimoAdminActivo(user))
                return Conflict(new { mensaje = "No se puede quitar el rol al último administrador activo." });

            var actuales = await _users.GetRolesAsync(user);
            await _users.RemoveFromRolesAsync(user, actuales);
            await _users.AddToRoleAsync(user, req.Rol);
            await _users.UpdateSecurityStampAsync(user);

            return Ok(new { mensaje = "Rol actualizado." });
        }

        // PUT api/admin/usuarios/{id}/estado
        [HttpPut("usuarios/{id}/estado")]
        public async Task<IActionResult> CambiarEstado(string id, CambiarEstadoRequest req)
        {
            if (req.Estado is not ("Activa" or "Deshabilitada"))
                return BadRequest(new { mensaje = "Estado inválido." });

            var user = await _users.FindByIdAsync(id);
            if (user is null) return NotFound();

            if (req.Estado == "Deshabilitada"
                && await _users.IsInRoleAsync(user, "Administrador")
                && await EsUltimoAdminActivo(user))
                return Conflict(new { mensaje = "No se puede deshabilitar al último administrador activo." });

            user.Estado = req.Estado;
            await _users.UpdateAsync(user);

            return Ok(new { mensaje = "Estado actualizado." });
        }

        private async Task<bool> EsUltimoAdminActivo(ApplicationUser objetivo)
        {
            var admins = await _users.GetUsersInRoleAsync("Administrador");
            var activos = admins.Where(a => a.Estado == "Activa").ToList();
            return activos.Count == 1 && activos[0].Id == objetivo.Id;
        }
    }
}
