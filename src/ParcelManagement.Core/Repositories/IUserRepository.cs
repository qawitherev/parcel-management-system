using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Repositories
{
    public interface IUserRepository
    {
        // for now we only focus whats in sprint, register and login

        Task<User?> GetUserByIdAsync(Guid id);

        Task<User> CreateUserAsync(User newUser);
    }
}
