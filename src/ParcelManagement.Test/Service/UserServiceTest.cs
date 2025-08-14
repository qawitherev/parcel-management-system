using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Misc;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Test.Fixture;
using Xunit;

namespace ParcelManagement.Test.Service
{
    public class UserServiceTest(UserTestAsyncLifetimeFixture fixture) : IClassFixture<UserTestAsyncLifetimeFixture>
    {
        private readonly UserTestAsyncLifetimeFixture _fixture = fixture;

        [Fact]
        public async Task UserRegisterAsync_UsernameAlreadyExist_ShouldThrowError()
        {
            var theUsername = "registered_username";
            var registeredUser = new User
            {
                Id = Guid.NewGuid(),
                Username = theUsername,
                Email = "this@email.com",
                ResidentUnit = "RU001",
                PasswordHash = "####",
                PasswordSalt = "salt",
            };

            await _fixture.DbContext.Users.AddAsync(registeredUser);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = theUsername,
                Email = "this_2@email.com",
                ResidentUnit = "RU001",
                PasswordHash = "####",
                PasswordSalt = "salt",
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _fixture.UserService.UserRegisterAsync(newUser.Username, "plainPassword", newUser.Email, "RU001");
            });
        }

        [Fact]
        public async Task UserRegisterAsync_UsernameValid_ShouldRegisterUser()
        {
            var theUsername = "registered_username";
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = theUsername,
                Email = "this@email.com",
                ResidentUnit = "RU001",
                PasswordHash = "####",
                PasswordSalt = "salt",
            };

            await _fixture.UserService.UserRegisterAsync(
                newUser.Username, "plainPassword", newUser.Email, "RU001"
            );
            var usernameSpec = new UserByUsernameSpecification(theUsername);
            var result = await _fixture.DbContext.Users.Where(usernameSpec.ToExpression())
                .FirstOrDefaultAsync();
            Assert.NotNull(result);
            Assert.Equal(theUsername, result.Username);
        }

        [Fact]
        public async Task UserLoginAsync_InvalidCredentials_ShouldFailedLogin()
        {
            var theUsername = "registered_username";
            var registeredUser = new User
            {
                Id = Guid.NewGuid(),
                Username = theUsername,
                Email = "this@email.com",
                ResidentUnit = "RU001",
                PasswordHash = "####",
                PasswordSalt = "salt",
            };

            await _fixture.DbContext.Users.AddAsync(registeredUser);

            await Assert.ThrowsAsync<InvalidCredentialException>(async () =>
            {
                await _fixture.UserService.UserLoginAsync(theUsername, "plainText");
            });
        }

        [Fact]
        public async Task UserLoginAsync_ValidLogin_ShouldLogin()
        {
            var theUsername = "registered_username_2";
            var plainPassword = "password123";
            var registeredUser = new User
            {
                Id = Guid.NewGuid(),
                Username = theUsername,
                Email = "this@email.com",
                ResidentUnit = "RU001",
                PasswordHash = "####",
                PasswordSalt = "salt",
            };
            var hashedPassword = PasswordService.HashPassword(registeredUser, plainPassword);
            registeredUser.PasswordHash = hashedPassword;
            await _fixture.DbContext.Users.AddAsync(registeredUser);
            await _fixture.DbContext.SaveChangesAsync();
            var result = await _fixture.UserService.UserLoginAsync(theUsername, plainPassword);
            Assert.NotNull(result);
        }
    }
}