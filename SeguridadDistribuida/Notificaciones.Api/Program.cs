using Microsoft.EntityFrameworkCore;
using Notificaciones.Api.Data;
using Notificaciones.Api.Security;
using Notificaciones.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NotificacionesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NotificacionesDb")));

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<CorreoService>();
builder.Services.AddScoped<ApiKeyFilter>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();