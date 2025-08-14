using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.DTO;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller
{

    [ApiController]
    [Route("api/[controller]")]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpGet("GetUserById/{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetUserById(id);
            return Ok(user);
        }

        [HttpPost("register/resident")]
        public async Task<IActionResult> RegisterResident([FromBody] RegisterResidentDto dto)
        {
            var newUser = await _userService.UserRegisterAsync(dto.Username, dto.PlainPassword, dto.Email, dto.ResidentUnit);
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
            await _userService.UserLoginAsync(dto.Username, dto.PlainPassword);
            // for now just return Ok - will do jwt thing later 
            return Ok(new string("hello world"));
        }
    }
}