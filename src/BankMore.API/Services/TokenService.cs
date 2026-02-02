using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BankMore.Shared.Interfaces;

namespace BankMore.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(string userId, string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var jwtSecret = _configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new ArgumentException("Chave JWT não configurada");
            }

            var key = Encoding.ASCII.GetBytes(jwtSecret);

            var expirationHours = _configuration.GetValue<int>("Jwt:ExpirationHours", 2);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("contaId", userId), // 🔥 CLAIM PERSONALIZADA
            new Claim("numeroConta", GetNumeroContaFromUserId(userId) ?? ""), // Opcional
            new Claim(ClaimTypes.Email, email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        }),
                Expires = DateTime.UtcNow.AddHours(expirationHours),
                Issuer = _configuration["Jwt:Issuer"] ?? "BankMoreAPI",
                Audience = _configuration["Jwt:Audience"] ?? "BankMoreClient",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string? ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecret = _configuration["Jwt:Secret"];

            if (string.IsNullOrEmpty(jwtSecret))
                return null;

            var key = Encoding.ASCII.GetBytes(jwtSecret);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"] ?? "BankMoreAPI",
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"] ?? "BankMoreClient",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

                return userId;
            }
            catch
            {
                return null;
            }
        }

        public string? GetUserIdFromToken(string token)
        {
            return ValidateToken(token);
        }

        public bool IsTokenValid(string token)
        {
            return !string.IsNullOrEmpty(ValidateToken(token));
        }

        private string? GetNumeroContaFromUserId(string userId)
        {
            return null;
        }
    }
}