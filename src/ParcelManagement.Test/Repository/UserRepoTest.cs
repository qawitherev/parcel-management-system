// TODO 
// To change this to using fixture 

using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Infrastructure.Repository;
using Xunit;

namespace ParcelManagement.Test.Repository
{
    public class UserRepositoryTest
    {

        // COMMENTED OUT BECAUSE IN MEMORY DATABASE DOES NOT ENFORCE 
        // DATA ANNOTATION
        // [Fact]
        // public async Task CreateUserAsync_ExceededMaxLength_ShouldError()
        // {
        //     var newInvalidUser = new User
        //     {
        //         Id = Guid.NewGuid(),
        //         Username = new string('a', 101),
        //         Email = "this@email.com",
        //         ResidentUnit = "RU001",
        //         PasswordHash = "####",
        //         PasswordSalt = "salt",
        //     };
        //     var testDbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
        //         .UseInMemoryDatabase(databaseName: "testDB").Options;

        //     using (var dbContext = new ApplicationDbContext(testDbContextOptions))
        //     {
        //         var userRepo = new UserRepository(dbContext);
        //         await Assert.ThrowsAsync<DbUpdateException>(async () =>
        //         {
        //             await userRepo.CreateUserAsync(newInvalidUser);
        //         });

        //     }
        // }

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

        [Fact]
        public async Task GetUserbyIdAsync_NonNull_ShouldReturnUser()
        {
            var theId = Guid.NewGuid();
            var newUser = new User
            {
                Id = theId,
                Username = "username",
                Email = "this@email.com",
                ResidentUnit = "RU001",
                PasswordHash = "####",
                PasswordSalt = "salt",
            };

            var dbContextOption = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "testDB").Options;

            using (var dbContext = new ApplicationDbContext(dbContextOption))
            {
                var userRepo = new UserRepository(dbContext);
                await dbContext.Users.AddAsync(newUser);
                await dbContext.SaveChangesAsync();
                var result = await userRepo.GetUserByIdAsync(theId);
                Assert.NotNull(result);
                Assert.Equal(theId, result.Id);
            }
        }

        [Fact]
        public async Task GetUserByIdAsync_Null_ShouldNotReturnUser()
        {
            var dbContextOption = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

            using (var dbContext = new ApplicationDbContext(dbContextOption))
            {
                var userRepo = new UserRepository(dbContext);
                var result = await userRepo.GetUserByIdAsync(Guid.NewGuid());
                Assert.Null(result);
            }
        }

        // this test is for username specification
        [Fact]
        public async Task GetOneUserBySpecification_NotNull_ShouldReturnUser()
        {
            var theUsername = "username hihi";
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = theUsername,
                Email = "this@email.com",
                ResidentUnit = "RU001",
                PasswordHash = "####",
                PasswordSalt = "salt",
            };

            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

            using (var dbContext = new ApplicationDbContext(dbContextOptions))
            {
                await dbContext.Users.AddAsync(newUser);
                await dbContext.SaveChangesAsync();
                var userRepo = new UserRepository(dbContext);
                var userByUsernameSpecification = new UserByUsernameSpecification(newUser.Username);
                var result = await userRepo.GetOneUserBySpecification(userByUsernameSpecification);
                Assert.NotNull(result);
                Assert.Equal(theUsername, result.Username);                
            }
        }
    }
}