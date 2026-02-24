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

        // 🔥 CAMBIADO: string rolNombre en lugar de int rolId
        public string GenerarToken(int usuNum, string nombre, string rolNombre)
        {
            var claims = new[]
            {
                new Claim("USU_NUM", usuNum.ToString()),
                new Claim(ClaimTypes.Name, nombre),
                new Claim(ClaimTypes.Role, rolNombre), // ✅ "Enoc"
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}