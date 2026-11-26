using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Usuarios.Api.Data;
using Usuarios.Api.DTOs;
using Usuarios.Api.Model;
using Usuarios.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace Usuarios.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PerfilesController : ControllerBase
    {
        private readonly UsuariosDbContext _db;

        public PerfilesController(UsuariosDbContext db) => _db = db;

        // ---------- Interno: Auth.Api crea el perfil al registrar ----------
        [HttpPost]
        [ServiceFilter(typeof(ApiKeyFilter))]
        public async Task<IActionResult> Crear(CrearPerfilRequest req)
        {
            if (await _db.Perfiles.AnyAsync(p => p.UsuarioId == req.UsuarioId || p.Correo == req.Correo))
                return Conflict(new { mensaje = "El perfil ya existe." });

            var perfil = new Perfil
            {
                UsuarioId = req.UsuarioId,
                Nombre = req.Nombre,
                Apellido = req.Apellido,
                Correo = req.Correo,
                FechaCreacion = DateTime.UtcNow
            };

            _db.Perfiles.Add(perfil);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerPorUsuario), new { usuarioId = perfil.UsuarioId }, perfil);
        }

        // ---------- Interno: compensación si falla el registro ----------
        [HttpDelete("{usuarioId}")]
        [ServiceFilter(typeof(ApiKeyFilter))]
        public async Task<IActionResult> Eliminar(string usuarioId)
        {
            var perfil = await _db.Perfiles.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);
            if (perfil is null) return NotFound();

            _db.Perfiles.Remove(perfil);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ---------- Mi perfil (JWT) ----------
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> MiPerfil()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var perfil = await _db.Perfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UsuarioId == id);
            return perfil is null ? NotFound() : Ok(perfil);
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> ActualizarMiPerfil(ActualizarPerfilRequest req)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var perfil = await _db.Perfiles.FirstOrDefaultAsync(p => p.UsuarioId == id);
            if (perfil is null) return NotFound();

            void Registrar(string campo, string? antes, string? despues)
            {
                if (antes == despues) return;
                _db.HistorialCambios.Add(new HistorialCambioPerfil
                {
                    UsuarioId = perfil.UsuarioId,
                    Campo = campo,
                    ValorAnterior = antes,
                    ValorNuevo = despues,
                    FechaCambio = DateTime.UtcNow
                });
            }

            Registrar("Nombre", perfil.Nombre, req.Nombre);
            Registrar("Apellido", perfil.Apellido, req.Apellido);
            Registrar("Telefono", perfil.Telefono, req.Telefono);
            Registrar("Direccion", perfil.Direccion, req.Direccion);
            Registrar("Ciudad", perfil.Ciudad, req.Ciudad);

            perfil.Nombre = req.Nombre;
            perfil.Apellido = req.Apellido;
            perfil.Telefono = req.Telefono;
            perfil.Direccion = req.Direccion;
            perfil.Ciudad = req.Ciudad;
            perfil.FechaActualizacion = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(perfil);
        }

        // ---------- Perfil por usuario: solo el dueño ----------
        [HttpGet("{usuarioId}")]
        [Authorize]
        public async Task<IActionResult> ObtenerPorUsuario(string usuarioId)
        {
            var idToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Un Cliente no puede ver perfiles ajenos (403). Los perfiles ajenos
            // se gestionan desde Auth.Api, según el documento.
            if (idToken != usuarioId)
                return Forbid();

            var perfil = await _db.Perfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);
            return perfil is null ? NotFound() : Ok(perfil);
        }
    }
}
