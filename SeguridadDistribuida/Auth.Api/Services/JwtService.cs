using Auth.Api.Model;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Auth.Api.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        public JwtService(IConfiguration config) => _config = config;

        public (string token, DateTime expira) Generar(ApplicationUser user, IList<string> roles)
        {
            var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Falta Jwt:Key");
            var minutos = _config.GetValue("Jwt:MinutosVigencia", 60);
            var expira = DateTime.UtcNow.AddMinutes(minutos);

            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                Expires = expira,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    SecurityAlgorithms.HmacSha256)
            };

            return (new JsonWebTokenHandler().CreateToken(descriptor), expira);
        }
    }
}
