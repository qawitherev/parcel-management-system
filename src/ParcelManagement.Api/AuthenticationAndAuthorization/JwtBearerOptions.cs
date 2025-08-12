using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ParcelManagement.Api.AuthenticationAndAuthorization
{
    public class JwtBearerConfiguration
    {
        // refactor the options as not to make the program.cs clutter with unnecessary stuffs 
        public static void JwtBearerOptionsConfig(JwtBearerOptions options, JWTSettings jwtSettings)
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey!))
            };
        }
    }
}  
