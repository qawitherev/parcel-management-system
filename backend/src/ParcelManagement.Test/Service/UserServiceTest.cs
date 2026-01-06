using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Misc;
using ParcelManagement.Core.Model.User;
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
                ResidentUnitDeprecated = "RU001",
                PasswordHash = "####",
                PasswordSalt = "salt",
            };

            await _fixture.DbContext.Users.AddAsync(registeredUser);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = theUsername,
                Email = "this_2@email.com",
                ResidentUnitDeprecated = "RU001",
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
                ResidentUnitDeprecated = "RU001",
                PasswordHash = "####",
                PasswordSalt = "salt",
            };
            await _fixture.DbContext.ResidentUnits.AddAsync(new ResidentUnit
            {
                Id = Guid.NewGuid(),
                UnitName = "RU001"
            });
            await _fixture.DbContext.SaveChangesAsync();

            var res = await _fixture.UserService.UserRegisterAsync(
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
                ResidentUnitDeprecated = "RU001",
                PasswordHash = "####",
                PasswordSalt = "salt",
            };

            await _fixture.DbContext.Users.AddAsync(registeredUser);

            await Assert.ThrowsAsync<InvalidCredentialException>(async () =>
            {
                var req = new UserLoginRequest
                {
                    Username = theUsername, 
                    Password = "plainText"
                };
                await _fixture.UserService.UserLoginAsync(req);
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
                ResidentUnitDeprecated = "RU001",
                PasswordHash = "####",
                PasswordSalt = "salt",
            };
            var hashedPassword = PasswordService.HashPassword(registeredUser, plainPassword);
            registeredUser.PasswordHash = hashedPassword;
            await _fixture.DbContext.Users.AddAsync(registeredUser);
            await _fixture.DbContext.SaveChangesAsync();
            var req = new UserLoginRequest
            {
                Username = theUsername, 
                Password = plainPassword
            };
            var result = await _fixture.UserService.UserLoginAsync(req);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_ValidToken_ShouldReturnUser()
        {
            // Arrange
            var refreshToken = "valid_refresh_token_12345";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "test_user",
                Email = "test@email.com",
                ResidentUnitDeprecated = "RU001",
                PasswordHash = "####",
                PasswordSalt = "salt",
                RefreshToken = refreshToken,
                RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7), // Valid for 7 days
                Role = UserRole.Resident
            };
            await _fixture.DbContext.Users.AddAsync(user);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.UserService.GetUserByRefreshTokenAsync(refreshToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(refreshToken, result.RefreshToken);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_ExpiredToken_ShouldReturnNull()
        {
            // Arrange
            var expiredRefreshToken = "expired_refresh_token_12345";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "test_user_expired",
                Email = "expired@email.com",
                ResidentUnitDeprecated = "RU002",
                PasswordHash = "####",
                PasswordSalt = "salt",
                RefreshToken = expiredRefreshToken,
                RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(-1), // Expired 1 day ago
                Role = UserRole.Resident
            };
            await _fixture.DbContext.Users.AddAsync(user);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.UserService.GetUserByRefreshTokenAsync(expiredRefreshToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_NonExistentToken_ShouldReturnNull()
        {
            // Arrange
            var nonExistentToken = "non_existent_token_12345";

            // Act
            var result = await _fixture.UserService.GetUserByRefreshTokenAsync(nonExistentToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_EmptyTokenInDatabase_ShouldReturnNull()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "test_user_empty_token",
                Email = "empty@email.com",
                ResidentUnitDeprecated = "RU003",
                PasswordHash = "####",
                PasswordSalt = "salt",
                RefreshToken = "", // Empty token
                RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7),
                Role = UserRole.Resident
            };
            await _fixture.DbContext.Users.AddAsync(user);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.UserService.GetUserByRefreshTokenAsync("");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_NullTokenInDatabase_ShouldReturnNull()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "test_user_null_token",
                Email = "null@email.com",
                ResidentUnitDeprecated = "RU004",
                PasswordHash = "####",
                PasswordSalt = "salt",
                RefreshToken = null, // Null token
                RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7),
                Role = UserRole.Resident
            };
            await _fixture.DbContext.Users.AddAsync(user);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.UserService.GetUserByRefreshTokenAsync("some_token");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_TokenExpiringToday_ShouldReturnNull()
        {
            // Arrange
            var tokenExpiringNow = "token_expiring_now_12345";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "test_user_expiring",
                Email = "expiring@email.com",
                ResidentUnitDeprecated = "RU005",
                PasswordHash = "####",
                PasswordSalt = "salt",
                RefreshToken = tokenExpiringNow,
                RefreshTokenExpiry = DateTimeOffset.UtcNow.AddMinutes(-1), // Expired 1 minute ago
                Role = UserRole.Resident
            };
            await _fixture.DbContext.Users.AddAsync(user);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.UserService.GetUserByRefreshTokenAsync(tokenExpiringNow);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_ValidTokenForParcelRoomManager_ShouldReturnUser()
        {
            // Arrange
            var refreshToken = "manager_refresh_token_12345";
            var manager = new User
            {
                Id = Guid.NewGuid(),
                Username = "manager_user",
                Email = "manager@email.com",
                ResidentUnitDeprecated = "",
                PasswordHash = "####",
                PasswordSalt = "salt",
                RefreshToken = refreshToken,
                RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(10),
                Role = UserRole.ParcelRoomManager
            };
            await _fixture.DbContext.Users.AddAsync(manager);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.UserService.GetUserByRefreshTokenAsync(refreshToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(manager.Id, result.Id);
            Assert.Equal(UserRole.ParcelRoomManager, result.Role);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_MultipleUsersOneValidToken_ShouldReturnCorrectUser()
        {
            // Arrange
            var validToken = "valid_token_user1";
            var user1 = new User
            {
                Id = Guid.NewGuid(),
                Username = "user1",
                Email = "user1@email.com",
                ResidentUnitDeprecated = "RU006",
                PasswordHash = "####",
                PasswordSalt = "salt",
                RefreshToken = validToken,
                RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7),
                Role = UserRole.Resident
            };
            var user2 = new User
            {
                Id = Guid.NewGuid(),
                Username = "user2",
                Email = "user2@email.com",
                ResidentUnitDeprecated = "RU007",
                PasswordHash = "####",
                PasswordSalt = "salt",
                RefreshToken = "different_token",
                RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7),
                Role = UserRole.Resident
            };
            await _fixture.DbContext.Users.AddRangeAsync(user1, user2);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.UserService.GetUserByRefreshTokenAsync(validToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user1.Id, result.Id);
            Assert.Equal("user1", result.Username);
        }

        
    }
}