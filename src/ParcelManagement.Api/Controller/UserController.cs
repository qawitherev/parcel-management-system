using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.AuthenticationAndAuthorization;
using ParcelManagement.Api.DTO;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller
{

    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    public class UserController(IUserService userService, ITokenService tokenService) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly ITokenService _tokenService = tokenService;

        [HttpGet("GetUserById/{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetUserById(id);
            return Ok(user);
        }

        [HttpPost("register/resident")]
        public async Task<IActionResult> RegisterResident([FromBody] RegisterResidentDto dto)
        {
            var newUser = await _userService.UserRegisterAsync(dto.Username, dto.Password, dto.Email, dto.ResidentUnit);
            var newUserDto = new UserResponseDto
            {
                Id = newUser.Id,
                Username = newUser.Username
            };
            return CreatedAtAction(nameof(GetUserById), new { id = newUserDto.Id }, newUserDto);
        }

        [HttpPost("login")]
        public async Task<IActionResult> UserLogin([FromBody] LoginDto dto)
        {
            var userId = await _userService.UserLoginAsync(dto.Username, dto.PlainPassword);
            Console.WriteLine($"user id is {userId!}");
            var jwt = _tokenService.GenerateToken(userId!, dto.Username);
            return Ok(new { Token = jwt });
        }
    }
}