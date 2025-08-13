using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Infrastructure.Repository;
using Xunit;

namespace ParcelManagement.Test.Repository
{
    public class UserRepositoryTest
    {
        [Fact]
        public async Task CreateUserAsync_ExceededMaxLength_ShouldError()
        {
            var newInvalidUser = new User
            {
                Id = Guid.NewGuid(),
                Username = new string('a', 101),
                Email = "this@email.com",
                ResidentUnit = "RU001",
                PasswordHash = "####",
                PasswordSalt = "salt",
            };
            var testDbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "testDB").Options;

            using (var dbContext = new ApplicationDbContext(testDbContextOptions))
            {
                var userRepo = new UserRepository(dbContext);
                await Assert.ThrowsAsync<DbUpdateException>(async () =>
                {
                    await userRepo.CreateUserAsync(newInvalidUser);
                });

            }
        }

        [Fact]
        public async Task CreateUserAsync_ValidUser_ShouldCreateUser()
        {
            var theId = Guid.NewGuid();
            var validUser = new User
            {
                Id = theId,
                Username = "username",
                Email = "this@email.com",
                ResidentUnit = "RU001",
                PasswordHash = "####",
                PasswordSalt = "salt",
            };

            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "testDB").Options;
            using (var dbContext = new ApplicationDbContext(dbContextOptions))
            {
                var userRepo = new UserRepository(dbContext);
                await userRepo.CreateUserAsync(validUser);

                var insertedUser = await dbContext.Users.FindAsync(theId);
                Assert.NotNull(insertedUser);
                Assert.Equal(theId, validUser.Id);
            }
        }
    }
}