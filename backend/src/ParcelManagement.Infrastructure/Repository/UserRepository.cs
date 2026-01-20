using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Infrastructure.Repository
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> CreateUserAsync(User newUser)
        {
            return await CreateAsync(newUser);
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<IReadOnlyList<User>> GetUsersBySpecificationAsync(ISpecification<User> spec)
        {
            return await GetBySpecificationAsync(spec);
        }

        public async Task<User?> GetOneUserBySpecification(ISpecification<User> spec)
        {
            return await GetOneBySpecificationAsync(spec);
        }

        public async Task<int> GetUsersCountBySpecification(ISpecification<User> specification)
        {
            return await GetCountBySpecificationAsync(specification);
        }

        public async Task<List<Guid>> GetInvalidUserId(List<Guid> userIds)
        {
            var validUserIds = await _dbContext.Users.Where(user => userIds.Contains(user.Id))
                .Select(user => user.Id).ToListAsync();
            return [.. userIds.Except(validUserIds)];     
        }

        public async Task UpdateUserAsync(User user)
        {
            await UpdateAsync(user);
        }
    }
}