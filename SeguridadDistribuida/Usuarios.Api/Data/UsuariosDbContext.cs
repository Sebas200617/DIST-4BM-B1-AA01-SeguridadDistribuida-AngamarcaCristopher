using Microsoft.EntityFrameworkCore;
using Usuarios.Api.Model;

namespace Usuarios.Api.Data
{
    public class UsuariosDbContext : DbContext
    {
        public UsuariosDbContext(DbContextOptions<UsuariosDbContext> options) : base(options) { }

        public DbSet<Perfil> Perfiles => Set<Perfil>();
        public DbSet<HistorialCambioPerfil> HistorialCambios => Set<HistorialCambioPerfil>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Perfil>().ToTable("Perfiles", "dbo");
            modelBuilder.Entity<HistorialCambioPerfil>().ToTable("HistorialCambiosPerfil", "dbo");
        }
    }
}