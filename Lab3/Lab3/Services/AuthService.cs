using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Lab3.Services
{
    public class AuthService(IConfiguration configuration)
    {
        private static readonly Dictionary<string, string> Users =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["user"] = "pass",
                ["admin"] = "admin"
            };

        public bool Validate(string username, string password) =>
            Users.TryGetValue(username, out var stored) && stored == password;

        public string GenerateJwt(string username)
        {
            var cfg = configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: cfg["Issuer"],
                audience: cfg["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
