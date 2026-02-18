using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Repositories
{
    public interface IUserRepository
    {
        // for now we only focus whats in sprint, register and login

        Task<User?> GetUserByIdAsync(Guid id);

        Task<User> CreateUserAsync(User newUser);

        Task UpdateUserAsync(User user);

        Task<IReadOnlyList<User>> GetUsersBySpecificationAsync(ISpecification<User> spec);

        Task<User?> GetOneUserBySpecification(ISpecification<User> spec);

        Task<int> GetUsersCountBySpecification(ISpecification<User> specification);

        Task<List<Guid>> GetInvalidUserId(List<Guid> userIds);
    }
}
