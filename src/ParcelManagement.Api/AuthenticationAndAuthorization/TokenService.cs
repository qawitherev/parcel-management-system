using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ParcelManagement.Api.AuthenticationAndAuthorization
{
    public interface ITokenService
    {
        //TODO
        // To include role for authorization
        string GenerateToken(string id, string username);
    }

    public class TokenService(IOptions<JWTSettings> jwtSettings_iOptions) : ITokenService
    {
        private readonly JWTSettings jwtSettings = jwtSettings_iOptions.Value;
        public string GenerateToken(string id, string username)
        {
            // make claim
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, id),
                new(ClaimTypes.Name, username)
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
    }
}