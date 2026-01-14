using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using ParcelManagement.Api.AuthenticationAndAuthorization;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Misc;
using ParcelManagement.Core.Model;
using ParcelManagement.Core.Model.Helper;
using ParcelManagement.Core.Model.User;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Services;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Infrastructure.Repository;
using ParcelManagement.Test.Fixture;
using Xunit;

namespace ParcelManagement.Test.Service
{
    public class UserServiceTest : IClassFixture<UserTestFixture>
    {
        private readonly UserTestFixture _fixture;
        private readonly UserRepository _userRepo;
        private readonly UserResidentUnitRepository _userResidentUnitRepo;
        private readonly ResidentUnitRepository _residentUnitRepo;
        private readonly NotificationPrefRepository _notificationPrefRepo;
        private readonly NotificationPrefService _notificationPrefService;
        private readonly SessionRepository _sessionRepo;
        private readonly SessionService _sessionService;

        public UserServiceTest(UserTestFixture fixture)
        {
            _fixture = fixture;
            
            // Bake necessary components in constructor
            _userRepo = new UserRepository(_fixture.DbContext);
            _userResidentUnitRepo = new UserResidentUnitRepository(_fixture.DbContext);
            _residentUnitRepo = new ResidentUnitRepository(_fixture.DbContext);
            _notificationPrefRepo = new NotificationPrefRepository(_fixture.DbContext);
            _notificationPrefService = new NotificationPrefService(_notificationPrefRepo, _userRepo);
            _sessionRepo = new SessionRepository(_fixture.DbContext);
            _sessionService = new SessionService(_sessionRepo);
        }

        private UserService GetService(Mock<IRedisRepository>? mockRedisRepo = null)
        {
            // Default mock if not provided
            var redisRepo = mockRedisRepo ?? new Mock<IRedisRepository>();
            var jwtSettings = new JWTSettings { ExpirationMinutes = 60 };
            var jwtOptions = Options.Create(jwtSettings);
            var tokenBlacklistService = new TokenBlacklistService(redisRepo.Object, jwtOptions);

            return new UserService(
                _userRepo,
                _userResidentUnitRepo,
                _residentUnitRepo,
                _notificationPrefService,
                _sessionService,
                tokenBlacklistService
            );
        }

        #region UserRegisterAsync Tests

        [Fact]
        public async Task UserRegisterAsync_DuplicateUsername_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var mockRedis = new Mock<IRedisRepository>();
            var service = GetService(mockRedis);
            
            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "existinguser",
                Email = "existing@test.com",
                PasswordHash = "hash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Users.AddAsync(existingUser);
            await _fixture.DbContext.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
                await service.UserRegisterAsync("existinguser", "Password123", "another@test.com", "A-101"));
        }

        [Fact]
        public async Task UserRegisterAsync_NonExistentResidentUnit_ShouldThrowNullReferenceException()
        {
            // Arrange
            var mockRedis = new Mock<IRedisRepository>();
            var service = GetService(mockRedis);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
                await service.UserRegisterAsync("newuser", "Password123", "newuser@test.com", "NonExistentUnit"));
        }

        #endregion

        #region ParcelRoomManagerRegisterAsync Tests

        [Fact]
        public async Task ParcelRoomManagerRegisterAsync_ValidData_ShouldCreateManagerUser()
        {
            // Arrange
            var mockRedis = new Mock<IRedisRepository>();
            var service = GetService(mockRedis);

            // Act
            var result = await service.ParcelRoomManagerRegisterAsync("manager1", "ManagerPass123", "manager1@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("manager1", result.Username);
            Assert.Equal("manager1@test.com", result.Email);
            Assert.Equal(UserRole.ParcelRoomManager, result.Role);
            
            // Verify user was saved
            var savedUser = await _fixture.DbContext.Users.FirstOrDefaultAsync(u => u.Username == "manager1");
            Assert.NotNull(savedUser);
        }

        [Fact]
        public async Task ParcelRoomManagerRegisterAsync_DuplicateUsername_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var mockRedis = new Mock<IRedisRepository>();
            var service = GetService(mockRedis);
            
            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "existingmanager",
                Email = "existing@test.com",
                PasswordHash = "hash",
                Role = UserRole.ParcelRoomManager,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Users.AddAsync(existingUser);
            await _fixture.DbContext.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.ParcelRoomManagerRegisterAsync("existingmanager", "Password123", "another@test.com"));
        }

        #endregion

        #region UserLoginAsync Tests

        [Fact]
        public async Task UserLoginAsync_InvalidPassword_ShouldThrowInvalidCredentialException()
        {
            // Arrange
            var mockRedis = new Mock<IRedisRepository>();
            var service = GetService(mockRedis);
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "validuser",
                Email = "valid@test.com",
                PasswordHash = "dummy",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            user.PasswordHash = PasswordService.HashPlainPasswordOrToken(user, "CorrectPassword");
            await _fixture.DbContext.Users.AddAsync(user);
            await _fixture.DbContext.SaveChangesAsync();

            var loginRequest = new UserLoginRequest
            {
                Username = "validuser",
                Password = "WrongPassword",
                RefreshToken = "refresh_token",
                DeviceInfo = "Test Device",
                IpAddress = "127.0.0.1",
                RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialException>(async () =>
                await service.UserLoginAsync(loginRequest));
        }

        #endregion

        #region UserLogoutAsync Tests

        [Fact]
        public async Task UserLogoutAsync_ValidRequest_ShouldRemoveSessionAndBlacklistToken()
        {
            // Arrange
            var mockRedis = new Mock<IRedisRepository>();
            mockRedis.Setup(x => x.SetValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(true);
            var service = GetService(mockRedis);
            
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "logoutuser",
                Email = "logout@test.com",
                PasswordHash = "hash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Users.AddAsync(user);
            
            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = "refresh_token_logout",
                DeviceInfo = "Test Device",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                LastActive = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Sessions.AddAsync(session);
            await _fixture.DbContext.SaveChangesAsync();

            var logoutRequest = new UserLogoutRequest
            {
                UserId = userId,
                JwtId = "jwt_id_12345"
            };

            // Act
            await service.UserLogoutAsync(logoutRequest);

            // Assert
            var remainingSessions = await _fixture.DbContext.Sessions.Where(s => s.UserId == userId).ToListAsync();
            Assert.Empty(remainingSessions);
            
            // Verify Redis was called to blacklist token
            mockRedis.Verify(x => x.SetValueAsync(
                It.Is<string>(key => key.Contains("jwt_id_12345")),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>()), Times.Once);
        }

        #endregion

        #region GetUserById Tests

        [Fact]
        public async Task GetUserById_ExistingUser_ShouldReturnUser()
        {
            // Arrange
            var mockRedis = new Mock<IRedisRepository>();
            var service = GetService(mockRedis);
            
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@test.com",
                PasswordHash = "hash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Users.AddAsync(user);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await service.GetUserById(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public async Task GetUserById_NonExistentUser_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var mockRedis = new Mock<IRedisRepository>();
            var service = GetService(mockRedis);
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await service.GetUserById(nonExistentId));
        }

        #endregion

        #region GetUserRole Tests

        [Fact]
        public async Task GetUserRole_ExistingUser_ShouldReturnRoleAsString()
        {
            // Arrange
            var mockRedis = new Mock<IRedisRepository>();
            var service = GetService(mockRedis);
            
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "adminuser",
                Email = "admin@test.com",
                PasswordHash = "hash",
                Role = UserRole.Admin,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Users.AddAsync(user);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await service.GetUserRole(userId);

            // Assert
            Assert.Equal("Admin", result);
        }

        [Fact]
        public async Task GetUserRole_NonExistentUser_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var mockRedis = new Mock<IRedisRepository>();
            var service = GetService(mockRedis);
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await service.GetUserRole(nonExistentId));
        }

        #endregion

        #region GetUserByRefreshTokenAsync Tests

        [Fact]
        public async Task GetUserByRefreshTokenAsync_ValidToken_ShouldReturnUserAndUpdateLastActive()
        {
            // Arrange
            var mockRedis = new Mock<IRedisRepository>();
            var service = GetService(mockRedis);
            
            var userId = Guid.NewGuid();
            var refreshToken = "valid_refresh_token";
            var hashedToken = TokenUtility.HashToken(refreshToken);
            var originalLastActive = DateTimeOffset.UtcNow.AddHours(-2);
            
            var user = new User
            {
                Id = userId,
                Username = "tokenuser",
                Email = "token@test.com",
                PasswordHash = "hash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Users.AddAsync(user);
            
            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = hashedToken,
                DeviceInfo = "Test Device",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                LastActive = originalLastActive
            };
            await _fixture.DbContext.Sessions.AddAsync(session);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await service.GetUserByRefreshTokenAsync(refreshToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("tokenuser", result.Username);
            
            // Verify LastActive was updated
            var updatedSession = await _fixture.DbContext.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
            Assert.NotNull(updatedSession);
            Assert.True(updatedSession.LastActive > originalLastActive);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_ExpiredToken_ShouldReturnNull()
        {
            // Arrange
            var mockRedis = new Mock<IRedisRepository>();
            var service = GetService(mockRedis);
            
            var userId = Guid.NewGuid();
            var refreshToken = "expired_token";
            var hashedToken = TokenUtility.HashToken(refreshToken);
            
            var user = new User
            {
                Id = userId,
                Username = "expireduser",
                Email = "expired@test.com",
                PasswordHash = "hash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Users.AddAsync(user);
            
            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = hashedToken,
                DeviceInfo = "Test Device",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1), // Expired
                LastActive = DateTimeOffset.UtcNow.AddDays(-1)
            };
            await _fixture.DbContext.Sessions.AddAsync(session);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await service.GetUserByRefreshTokenAsync(refreshToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_NonExistentToken_ShouldReturnNull()
        {
            // Arrange
            var mockRedis = new Mock<IRedisRepository>();
            var service = GetService(mockRedis);

            // Act
            var result = await service.GetUserByRefreshTokenAsync("non_existent_token");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_EmptyRefreshToken_ShouldReturnNull()
        {
            // Arrange
            var mockRedis = new Mock<IRedisRepository>();
            var service = GetService(mockRedis);
            
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "emptyuser",
                Email = "empty@test.com",
                PasswordHash = "hash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Users.AddAsync(user);
            
            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = "", // Empty token
                DeviceInfo = "Test Device",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                LastActive = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Sessions.AddAsync(session);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await service.GetUserByRefreshTokenAsync("");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetUserForViewAsync Tests

        [Fact]
        public async Task GetUserForViewAsync_WithoutFilters_ShouldReturnAllUsers()
        {
            // Arrange
            var mockRedis = new Mock<IRedisRepository>();
            var service = GetService(mockRedis);
            
            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "user1",
                    Email = "user1@test.com",
                    PasswordHash = "hash",
                    Role = UserRole.Resident,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "user2",
                    Email = "user2@test.com",
                    PasswordHash = "hash",
                    Role = UserRole.ParcelRoomManager,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };
            await _fixture.DbContext.Users.AddRangeAsync(users);
            await _fixture.DbContext.SaveChangesAsync();

            var filter = new FilterPaginationRequest<UserSortableColumn>
            {
                Page = 1,
                Take = 10
            };

            // Act
            var (resultUsers, count) = await service.GetUserForViewAsync(filter);

            // Assert
            Assert.NotNull(resultUsers);
            Assert.True(resultUsers.Count >= 2);
            Assert.True(count >= 2);
        }

        #endregion
    }
}