using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;

namespace ParcelManagement.Core.Services
{
    public interface IUserService
    {
        Task<User> UserRegisterAsync(string username, string password, string email, UserRole role);

        Task<string?> UserLoginAsync(string username, string password);
    }

    public class UserService(IUserRepository userRepository) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        public Task<string?> UserLoginAsync(string username, string password)
        {
            throw new NotImplementedException();
        }

        public async Task<User> UserRegisterAsync(string username, string password, string email, UserRole role)
        {
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                PasswordHash = "will do this later",
                PasswordSalt = "will do this later",
                Role = role,
                CreatedAt = DateTimeOffset.UtcNow
            };

            // TODO 
            // make sure username dont conflict --> do migration as well as manual checking 
            // make sure password follow rule 
            // make sure email is valid 

            return await _userRepository.CreateUserAsync(newUser);

        }
    }
}