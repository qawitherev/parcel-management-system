using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // for now we will just make one user = one role
            // no keperluan for multiple roles for now 
            var loginRes = await _userService.UserLoginAsync(dto.Username, dto.PlainPassword);
            var jwt = _tokenService.GenerateToken(loginRes[0], dto.Username, loginRes[1]);
            return Ok(new { Token = jwt });
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
                Username = r.Username
            };
            return CreatedAtAction(nameof(GetUserById), new { id = newUserDto.Id }, newUserDto);
        }
    }
}