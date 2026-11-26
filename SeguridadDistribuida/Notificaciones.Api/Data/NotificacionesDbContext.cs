using Microsoft.EntityFrameworkCore;
using Notificaciones.Api.Model;

namespace Notificaciones.Api.Data
{
    public class NotificacionesDbContext : DbContext
    {
        public NotificacionesDbContext(DbContextOptions<NotificacionesDbContext> options)
            : base(options) { }

        public DbSet<EnvioCorreo> EnviosCorreo => Set<EnvioCorreo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EnvioCorreo>().ToTable("EnviosCorreo", "dbo");
        }
    }
}
