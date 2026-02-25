using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Davivienda.GraphQL.Security
{
    public class JwtProvider
    {
        private readonly IConfiguration _configuration;

        public JwtProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerarToken(int usuNum, string nombre, string rolNombre)
        {
            var claims = new[]
            {
                new Claim("USU_NUM", usuNum.ToString()),
                new Claim(ClaimTypes.Name, nombre),
                new Claim(ClaimTypes.Role, rolNombre),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // 🔥 GUID único por token
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()) // 🔥 Timestamp de creación
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6), // 🔥 6 HORAS DE DURACIÓN
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}