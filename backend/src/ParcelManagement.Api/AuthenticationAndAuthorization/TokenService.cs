using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ParcelManagement.Api.AuthenticationAndAuthorization
{
    public interface ITokenService
    {
        string GenerateAccessToken(string id, string username, string role);

        string GenerateRefreshToken();

        DateTimeOffset GetRefreshTokenExpiry(int days);
    }

    public class TokenService(IOptions<JWTSettings> jwtSettings_iOptions) : ITokenService
    {
        private readonly JWTSettings jwtSettings = jwtSettings_iOptions.Value;
        public string GenerateAccessToken(string id, string username, string role)
        {
            if (jwtSettings.SecretKey == null)
            {
                throw new NullReferenceException("Secret key is not exist");
            }
            // make claim
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, id),
                new(ClaimTypes.Name, username),
                new(ClaimTypes.Role, role)
            };
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey!));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var tokenRaw = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes((double)jwtSettings.ExpirationMinutes!),
                signingCredentials: signingCredentials
            );
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenRaw);
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
        public DateTimeOffset GetRefreshTokenExpiry(int days)
        {
            return DateTimeOffset.UtcNow.AddDays(days);
        }
    }
}