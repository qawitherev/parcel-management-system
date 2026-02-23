using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.AuthenticationAndAuthorization;
using ParcelManagement.Api.CustomAttribute;
using ParcelManagement.Api.DTO;
using ParcelManagement.Api.DTO.V1;
using ParcelManagement.Api.Utility;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model;
using ParcelManagement.Core.Model.Helper;
using ParcelManagement.Core.Model.User;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller
{

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Consumes("application/json")]
    public class UserController(IUserService userService, ITokenService tokenService,
        IUserContextService userContextService
    ) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IUserContextService _userContextService = userContextService;
        // TODO: we might want to use config file for this one 👇🏽
        private readonly int REFRESH_TOKEN_EXPIRY_DAYS = 10;

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetUserById(id);
            return Ok(user);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetUserByIdAsync()
        {
            var userId = _userContextService.GetUserId();
            var u = await _userService.GetUserById(userId);
            return Ok(new UserResponseDto
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role.ToString()
            });
        }

        /**
            REGISTER USER ENDPOINTS 
        **/
        [HttpPost("register/resident")]
        public async Task<IActionResult> RegisterResident([FromBody] RegisterResidentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newUser = await _userService.UserRegisterAsync(dto.Username, dto.Password, dto.Email, dto.ResidentUnit);
            var newUserDto = new UserResponseDto
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Role = newUser.Role.ToString()
            };
            return CreatedAtAction(nameof(GetUserById), new { id = newUserDto.Id }, newUserDto);
        }

        [HttpPost("register/parcelRoomManager")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterParcelRoomAdmin([FromBody] RegisterParceRoomManagerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var r = await _userService.ParcelRoomManagerRegisterAsync(
                dto.Username, dto.Password, dto.Email
            );
            var newUserDto = new UserResponseDto
            {
                Id = r.Id,
                Username = r.Username,
                Role = r.Role.ToString()
            };
            return CreatedAtAction(nameof(GetUserById), new { id = newUserDto.Id }, newUserDto);
        }


        [HttpPost("login")]
        [SkipBlacklistCheck]
        public async Task<IActionResult> UserLogin([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var refreshToken = _tokenService.GenerateRefreshToken();
            var RefreshTokenExpiry = _tokenService.GetRefreshTokenExpiry(REFRESH_TOKEN_EXPIRY_DAYS);
            var loginRequest = new UserLoginRequest
            {
                Username = dto.Username, 
                Password = dto.PlainPassword, 
                RefreshToken = refreshToken,
                RefreshTokenExpiry = RefreshTokenExpiry,
                DeviceInfo = HttpContextUtilities.GetDeviceInfo(HttpContext), 
                IpAddress = HttpContextUtilities.GetDeviceIp(HttpContext)
            };
            var loginRes = await _userService.UserLoginAsync(loginRequest);
            var jwt = _tokenService.GenerateAccessToken(loginRes[0], dto.Username, loginRes[1]);
            
            var loginReponseDto = new LoginResponseDto
            {
                AccessToken = jwt
            };
            
            var cookieOption = new CookieOptions
            {
                HttpOnly = true, 
                SameSite = SameSiteMode.Lax, 
                Secure = false,
                Expires = RefreshTokenExpiry
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOption);

            return Ok(loginReponseDto);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> UserLogout()
        {
            var userId = _userContextService.GetUserId();
            var jwtId = _userContextService.GetTokenId();
            var request = new UserLogoutRequest
            {
                UserId = userId, 
                JwtId = jwtId ?? ""
            };
            await _userService.UserLogoutAsync(request);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, 
                SameSite = SameSiteMode.Lax, 
                Secure = false,
                Expires = DateTimeOffset.UtcNow.AddDays(-(double)REFRESH_TOKEN_EXPIRY_DAYS)
            };
            Response.Cookies.Append("refreshToken", "", cookieOptions);
            return Ok();
        }

        [HttpGet("")]
        [Authorize(Roles = "ParcelRoomManager, Admin")]
        public async Task<IActionResult> GetUsersForView([FromQuery] BaseFilterDto dto)
        {
            var sortableColumn = EnumUtils.ToEnumOrNull<UserSortableColumn>(dto.Column ?? "");
            var filter = new FilterPaginationRequest<UserSortableColumn>
            {
                SearchKeyword = dto.SearchKeyword,
                Page = dto.Page,
                Take = dto.Take,
                SortableColumn = sortableColumn ?? UserSortableColumn.Username,
                IsAscending = dto.IsAscending
            };
            var (users, count) = await _userService.GetUserForViewAsync(filter);
            var responseDto = new UsersResponseDto
            {
                Count = count,
                Users = [.. users.Select(user => new UserResponseDto {
                    Id = user.Id,
                    Role = user.Role.ToString(),
                    Username = user.Username
                })]
            };
            return Ok(responseDto);
        }
        
    }
    
}