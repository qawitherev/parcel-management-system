using System.Security.Authentication;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Misc;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface IUserService
    {
        Task<User> UserRegisterAsync(string username, string password, string email, string residentUnit);

        Task<string?> UserLoginAsync(string username, string password);

        Task<User?> GetUserById(Guid id);
    }

    public class UserService(IUserRepository userRepository) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        public async Task<string?> UserLoginAsync(string username, string password)
        {
            var userByUsernameSpec = new UserByUsernameSpecification(username);
            var possibleUser = await _userRepository.GetOneUserBySpecification(userByUsernameSpec) ?? throw new InvalidCredentialException($"Invalid login credential - username");
            var isCredentialValid = PasswordService.VerifyPassword(possibleUser, possibleUser.PasswordHash, password);
            if (!isCredentialValid)
            {
                throw new InvalidCredentialException("Invalid login credentials");
            }
            return possibleUser.Id.ToString();
        }

        // this service is only to register resident 
        public async Task<User> UserRegisterAsync(string username, string password, string email, string residentUnit)
        {
            // TODO 
            // make sure username dont conflict --> do migration as well as manual checking 
            var userByUsernameSpec = new UserByUsernameSpecification(username);
            var existingUser = await _userRepository.GetOneUserBySpecification(userByUsernameSpec);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with username {username} has already exist.");
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                ResidentUnit = residentUnit,
                PasswordHash = "will do this later",
                PasswordSalt = "will do this later",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var hashedPassword = PasswordService.HashPassword(newUser, password);
            newUser.PasswordHash = hashedPassword;

            // make sure email is valid 

            return await _userRepository.CreateUserAsync(newUser);

        }

        public async Task<User?> GetUserById(Guid id)
        {
            return await _userRepository.GetUserByIdAsync(id) ?? throw new KeyNotFoundException($"User with id {id} is not found");
        }
    }
}