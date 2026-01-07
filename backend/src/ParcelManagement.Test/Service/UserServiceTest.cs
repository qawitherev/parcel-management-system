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
            var hashedPassword = PasswordService.HashPlainPasswordOrToken(registeredUser, plainPassword);
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
            var userId = Guid.NewGuid();
            var username = "test_user_valid";
            var refreshToken = "valid_refresh_token_12345";
            
            var user = new User
            {
                Id = userId,
                Username = username,
                Email = "valid@email.com",
                ResidentUnitDeprecated = "RU001",
                PasswordHash = "####",
                PasswordSalt = "salt",
                Role = UserRole.Resident
            };
            await _fixture.DbContext.Users.AddAsync(user);

            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = refreshToken,
                DeviceInfo = "TestDevice",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                LastActive = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Sessions.AddAsync(session);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.UserService.GetUserByRefreshTokenAsync(refreshToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal(username, result.Username);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_ExpiredToken_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expiredRefreshToken = "expired_refresh_token_12345";
            
            var user = new User
            {
                Id = userId,
                Username = "test_user_expired",
                Email = "expired@email.com",
                ResidentUnitDeprecated = "RU002",
                PasswordHash = "####",
                PasswordSalt = "salt",
                Role = UserRole.Resident
            };
            await _fixture.DbContext.Users.AddAsync(user);

            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = expiredRefreshToken,
                DeviceInfo = "ExpiredDevice",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1), // Expired 1 day ago
                LastActive = DateTimeOffset.UtcNow.AddDays(-1)
            };
            await _fixture.DbContext.Sessions.AddAsync(session);
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
        public async Task GetUserByRefreshTokenAsync_EmptyTokenInSession_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            
            var user = new User
            {
                Id = userId,
                Username = "test_user_empty_token",
                Email = "empty@email.com",
                ResidentUnitDeprecated = "RU003",
                PasswordHash = "####",
                PasswordSalt = "salt",
                Role = UserRole.Resident
            };
            await _fixture.DbContext.Users.AddAsync(user);

            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = "", // Empty token
                DeviceInfo = "EmptyTokenDevice",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                LastActive = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Sessions.AddAsync(session);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.UserService.GetUserByRefreshTokenAsync("");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_NullTokenInSession_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            
            var user = new User
            {
                Id = userId,
                Username = "test_user_null_token",
                Email = "null@email.com",
                ResidentUnitDeprecated = "RU004",
                PasswordHash = "####",
                PasswordSalt = "salt",
                Role = UserRole.Resident
            };
            await _fixture.DbContext.Users.AddAsync(user);

            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = null, // Null token
                DeviceInfo = "NullTokenDevice",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                LastActive = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Sessions.AddAsync(session);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.UserService.GetUserByRefreshTokenAsync("some_token");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_TokenExpiredOneMinuteAgo_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var tokenExpiringNow = "token_expiring_now_12345";
            
            var user = new User
            {
                Id = userId,
                Username = "test_user_expiring",
                Email = "expiring@email.com",
                ResidentUnitDeprecated = "RU005",
                PasswordHash = "####",
                PasswordSalt = "salt",
                Role = UserRole.Resident
            };
            await _fixture.DbContext.Users.AddAsync(user);

            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = tokenExpiringNow,
                DeviceInfo = "ExpiringDevice",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1), // Expired 1 minute ago
                LastActive = DateTimeOffset.UtcNow.AddMinutes(-1)
            };
            await _fixture.DbContext.Sessions.AddAsync(session);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.UserService.GetUserByRefreshTokenAsync(tokenExpiringNow);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_MultipleSessionsSameUser_ShouldReturnCorrectSession()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var username = "multi_session_user";
            var refreshToken1 = "refresh_token_session_1";
            var refreshToken2 = "refresh_token_session_2";
            
            var user = new User
            {
                Id = userId,
                Username = username,
                Email = "multisession@email.com",
                ResidentUnitDeprecated = "RU006",
                PasswordHash = "####",
                PasswordSalt = "salt",
                Role = UserRole.Resident
            };
            await _fixture.DbContext.Users.AddAsync(user);

            var session1 = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = refreshToken1,
                DeviceInfo = "Device1",
                IpAddress = "192.168.1.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                LastActive = DateTimeOffset.UtcNow
            };

            var session2 = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = refreshToken2,
                DeviceInfo = "Device2",
                IpAddress = "192.168.1.2",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                LastActive = DateTimeOffset.UtcNow
            };

            await _fixture.DbContext.Sessions.AddRangeAsync(session1, session2);
            await _fixture.DbContext.SaveChangesAsync();

            // Act - Get user with first token
            var result1 = await _fixture.UserService.GetUserByRefreshTokenAsync(refreshToken1);

            // Assert
            Assert.NotNull(result1);
            Assert.Equal(userId, result1.Id);
            Assert.Equal(username, result1.Username);

            // Act - Get user with second token
            var result2 = await _fixture.UserService.GetUserByRefreshTokenAsync(refreshToken2);

            // Assert
            Assert.NotNull(result2);
            Assert.Equal(userId, result2.Id);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_DifferentUsersWithDifferentTokens_ShouldReturnCorrectUser()
        {
            // Arrange
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            var refreshToken1 = "user1_refresh_token";
            var refreshToken2 = "user2_refresh_token";
            
            var user1 = new User
            {
                Id = user1Id,
                Username = "user_one",
                Email = "user1@email.com",
                ResidentUnitDeprecated = "RU007",
                PasswordHash = "####",
                PasswordSalt = "salt",
                Role = UserRole.Resident
            };

            var user2 = new User
            {
                Id = user2Id,
                Username = "user_two",
                Email = "user2@email.com",
                ResidentUnitDeprecated = "RU008",
                PasswordHash = "####",
                PasswordSalt = "salt",
                Role = UserRole.Admin
            };

            await _fixture.DbContext.Users.AddRangeAsync(user1, user2);

            var session1 = new Session
            {
                Id = Guid.NewGuid(),
                UserId = user1Id,
                RefreshToken = refreshToken1,
                DeviceInfo = "User1Device",
                IpAddress = "192.168.1.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                LastActive = DateTimeOffset.UtcNow
            };

            var session2 = new Session
            {
                Id = Guid.NewGuid(),
                UserId = user2Id,
                RefreshToken = refreshToken2,
                DeviceInfo = "User2Device",
                IpAddress = "192.168.1.2",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                LastActive = DateTimeOffset.UtcNow
            };

            await _fixture.DbContext.Sessions.AddRangeAsync(session1, session2);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result1 = await _fixture.UserService.GetUserByRefreshTokenAsync(refreshToken1);
            var result2 = await _fixture.UserService.GetUserByRefreshTokenAsync(refreshToken2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Equal(user1Id, result1.Id);
            Assert.Equal(user2Id, result2.Id);
            Assert.NotEqual(result1.Id, result2.Id);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_ValidTokenUpdatesLastActive_ShouldUpdateTimestamp()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var refreshToken = "token_to_update_last_active";
            var originalLastActive = DateTimeOffset.UtcNow.AddHours(-2);
            
            var user = new User
            {
                Id = userId,
                Username = "test_user_last_active",
                Email = "lastactive@email.com",
                ResidentUnitDeprecated = "RU009",
                PasswordHash = "####",
                PasswordSalt = "salt",
                Role = UserRole.Resident
            };
            await _fixture.DbContext.Users.AddAsync(user);

            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = refreshToken,
                DeviceInfo = "UpdateDevice",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                LastActive = originalLastActive
            };
            await _fixture.DbContext.Sessions.AddAsync(session);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.UserService.GetUserByRefreshTokenAsync(refreshToken);

            // Assert
            Assert.NotNull(result);
            
            // Verify LastActive was updated
            var updatedSession = await _fixture.DbContext.Sessions
                .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);
            Assert.NotNull(updatedSession);
            Assert.True(updatedSession.LastActive > originalLastActive);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_OneSessionExpiredOneValid_ShouldReturnOnlyValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expiredToken = "expired_session_token";
            var validToken = "valid_session_token";
            
            var user = new User
            {
                Id = userId,
                Username = "mixed_sessions_user",
                Email = "mixedsessions@email.com",
                ResidentUnitDeprecated = "RU010",
                PasswordHash = "####",
                PasswordSalt = "salt",
                Role = UserRole.Resident
            };
            await _fixture.DbContext.Users.AddAsync(user);

            var expiredSession = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = expiredToken,
                DeviceInfo = "ExpiredDevice",
                IpAddress = "192.168.1.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1), // Expired
                LastActive = DateTimeOffset.UtcNow.AddDays(-1)
            };

            var validSession = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = validToken,
                DeviceInfo = "ValidDevice",
                IpAddress = "192.168.1.2",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7), // Valid
                LastActive = DateTimeOffset.UtcNow
            };

            await _fixture.DbContext.Sessions.AddRangeAsync(expiredSession, validSession);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var expiredResult = await _fixture.UserService.GetUserByRefreshTokenAsync(expiredToken);
            var validResult = await _fixture.UserService.GetUserByRefreshTokenAsync(validToken);

            // Assert
            Assert.Null(expiredResult); // Expired should return null
            Assert.NotNull(validResult); // Valid should return user
            Assert.Equal(userId, validResult.Id);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_TokenExpiringInFuture_ShouldReturnUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var futureToken = "future_expiring_token";
            
            var user = new User
            {
                Id = userId,
                Username = "future_user",
                Email = "future@email.com",
                ResidentUnitDeprecated = "RU011",
                PasswordHash = "####",
                PasswordSalt = "salt",
                Role = UserRole.ParcelRoomManager
            };
            await _fixture.DbContext.Users.AddAsync(user);

            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = futureToken,
                DeviceInfo = "FutureDevice",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(30), // Expires in 30 days
                LastActive = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Sessions.AddAsync(session);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.UserService.GetUserByRefreshTokenAsync(futureToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        }
       
    }
}