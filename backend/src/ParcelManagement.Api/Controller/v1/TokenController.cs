using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.AuthenticationAndAuthorization;
using ParcelManagement.Api.CustomAttribute;
using ParcelManagement.Api.DTO.V1;
using ParcelManagement.Api.Utility;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Consumes("application/json")]
    [SkipBlacklistCheck]
    public class TokenController(IUserService userService, ITokenService tokenService): ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly ITokenService _tokenService = tokenService;

        [HttpPost("refresh")]
        public async Task<IActionResult> TokenRefresh() 
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var userValid = await _userService.GetUserByRefreshTokenAsync(refreshToken ?? "");
            if (userValid == null)
            {
                return Unauthorized($"Invalid or expired token");
            }
            var newAccessToken = _tokenService.GenerateAccessToken(userValid.Id.ToString(), userValid.Username, userValid.Role.ToString());
            return Ok(new { accessToken = newAccessToken});
        }
    }
}