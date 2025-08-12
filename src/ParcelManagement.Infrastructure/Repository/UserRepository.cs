using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Infrastructure.Repository
{
    public class UserRepository(ApplicationDbContext dbContext) : IUserRepository
    {
        public async Task<User> CreateUserAsync(User newUser)
        {
            await dbContext.Users.AddAsync(newUser);
            await dbContext.SaveChangesAsync();
            return newUser;
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await dbContext.Users.FindAsync(id);
        }

        public async Task<IReadOnlyList<User?>> GetUsersBySpecificationAsync(ISpecification<User> spec)
        {
            return await dbContext.Users.Where(spec.ToExpression()).ToListAsync();
        }

        public async Task<User?> GetOneUserBySpecification(ISpecification<User> spec)
        {
            return await dbContext.Users.Where(spec.ToExpression()).FirstOrDefaultAsync();
        }
    }
}