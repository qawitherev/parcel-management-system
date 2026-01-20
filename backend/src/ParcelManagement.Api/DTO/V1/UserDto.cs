using System.ComponentModel.DataAnnotations;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Api.DTO.V1
{
    public class LoginDto
    {
        [Required]
        public required string Username { get; set; }
        [Required]
        public required string PlainPassword { get; set; }
    }

    public class LoginResponseDto
    {
        [Required]
        public required string AccessToken { get; set; }
    }

    public class RegisterResidentDto
    {
        [Required]
        public required string Username { get; set; }
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string ResidentUnit { get; set; }
        [Required]
        public required string Password { get; set; }
    }

    public class RegisterParceRoomManagerDto
    {
        [Required]
        public required string Username { get; set; }
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }
    }

    public class UsersResponseDto
    {
        public int Count { get; set; }
        public required List<UserResponseDto> Users { get; set; }
    }

    public class UserResponseDto
    {
        public required Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Role { get; set; }
    }

    public class RefreshTokenDto
    {
        public required string RefreshToken { get; set; }
    }
}