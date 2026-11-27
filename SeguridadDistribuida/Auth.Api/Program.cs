using System.Text;
using Auth.Api.Data;
using Auth.Api.Model;
using Auth.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection();

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDb")));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(o =>
    o.TokenLifespan = TimeSpan.FromMinutes(30));

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Falta Jwt:Key en User Secrets");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// IHttpClientFactory con BaseAddress y Timeout explícitos
void ConfigurarCliente(HttpClient client, string urlKey, string apiKeyKey)
{
    var direccion = builder.Configuration[urlKey]
        ?? throw new InvalidOperationException($"Falta {urlKey}");
    client.BaseAddress = new Uri(direccion);
    client.Timeout = TimeSpan.FromSeconds(builder.Configuration.GetValue("Servicios:TimeoutSegundos", 5));

    var clave = builder.Configuration[apiKeyKey];
    if (!string.IsNullOrEmpty(clave))
        client.DefaultRequestHeaders.Add("X-Service-Key", clave);
}

builder.Services.AddHttpClient("UsuariosApi", c => ConfigurarCliente(c, "Servicios:UsuariosApi", "Servicios:UsuariosApiKey"));
builder.Services.AddHttpClient("NotificacionesApi", c => ConfigurarCliente(c, "Servicios:NotificacionesApi", "Servicios:NotificacionesApiKey"));

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<UsuariosClient>();
builder.Services.AddScoped<NotificacionesClient>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();