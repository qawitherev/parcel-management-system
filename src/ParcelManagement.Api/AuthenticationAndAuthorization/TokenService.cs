using Microsoft.Extensions.Options;

namespace ParcelManagement.Api.AuthenticationAndAuthorization
{
    public interface ITokenService
    {
        //TODO
        // To include role for authorization
        Task<string> GenerateToken(Guid id, string username);
    }

    // public class TokenService(IOptions<JWTSettings> jwtSettings) : ITokenService
    // {
    //     private readonly IOptions<JWTSettings> _jwtSettings = jwtSettings;
    //     public Task<string> GenerateToken(Guid id, string username)
    //     {
    //         // general overview 
    //         // make claim
    //         // make token
    //         // write token --> returns token string 
    //     }
    // }
}