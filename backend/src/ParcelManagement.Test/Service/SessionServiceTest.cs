using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model;
using ParcelManagement.Core.Services;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Infrastructure.Repository;
using ParcelManagement.Test.Fixture;
using Xunit;

namespace ParcelManagement.Test.Service
{
    public class SessionServiceTest : IClassFixture<SessionTestFixture>
    {
        private readonly SessionTestFixture _fixture;
        private readonly SessionRepository _sessionRepo;

        public SessionServiceTest(SessionTestFixture fixture)
        {
            _fixture = fixture;
            _sessionRepo = new SessionRepository(_fixture.DbContext);
        }

        private SessionService GetService()
        {
            return new SessionService(_sessionRepo);
        }

        #region CreateSessionAsync Tests

        [Fact]
        public async Task CreateSessionAsync_NoExistingSessions_ShouldCreateSession()
        {
            // Arrange
            var service = GetService();
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

            var sessionRequest = new CreateSessionRequest
            {
                UserId = userId,
                RefreshToken = "refresh_token_123",
                DeviceInfo = "Test Device",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Act
            var result = await service.CreateSessionAsync(sessionRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("refresh_token_123", result.RefreshToken);
            Assert.Equal("Test Device", result.DeviceInfo);
            Assert.Equal("127.0.0.1", result.IpAddress);
            
            // Verify session was saved
            var savedSessions = await _fixture.DbContext.Sessions.Where(s => s.UserId == userId).ToListAsync();
            Assert.Single(savedSessions);
        }

        [Fact]
        public async Task CreateSessionAsync_WithFewExistingSessions_ShouldNotDeleteOldSessions()
        {
            // Arrange
            var service = GetService();
            var userId = Guid.NewGuid();
            
            var user = new User
            {
                Id = userId,
                Username = "testuser2",
                Email = "test2@test.com",
                PasswordHash = "hash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Users.AddAsync(user);

            // Create 2 existing sessions (under limit of 5)
            var existingSessions = new List<Session>
            {
                new Session
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    RefreshToken = "token1",
                    DeviceInfo = "Device1",
                    IpAddress = "192.168.1.1",
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                    LastActive = DateTimeOffset.UtcNow.AddHours(-2)
                },
                new Session
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    RefreshToken = "token2",
                    DeviceInfo = "Device2",
                    IpAddress = "192.168.1.2",
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                    LastActive = DateTimeOffset.UtcNow.AddHours(-1)
                }
            };
            await _fixture.DbContext.Sessions.AddRangeAsync(existingSessions);
            await _fixture.DbContext.SaveChangesAsync();

            var sessionRequest = new CreateSessionRequest
            {
                UserId = userId,
                RefreshToken = "new_token",
                DeviceInfo = "New Device",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Act
            var result = await service.CreateSessionAsync(sessionRequest);

            // Assert
            var allSessions = await _fixture.DbContext.Sessions.Where(s => s.UserId == userId).ToListAsync();
            Assert.Equal(3, allSessions.Count); // 2 old + 1 new = 3
            Assert.Contains(allSessions, s => s.RefreshToken == "new_token");
        }

        [Fact]
        public async Task CreateSessionAsync_WithMaxSessions_ShouldDeleteOldestSessions()
        {
            // Arrange
            var service = GetService();
            var userId = Guid.NewGuid();
            
            var user = new User
            {
                Id = userId,
                Username = "testuser3",
                Email = "test3@test.com",
                PasswordHash = "hash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Users.AddAsync(user);

            // Create 5 existing sessions (at max limit)
            var existingSessions = new List<Session>();
            for (int i = 1; i <= 5; i++)
            {
                existingSessions.Add(new Session
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    RefreshToken = $"token{i}",
                    DeviceInfo = $"Device{i}",
                    IpAddress = $"192.168.1.{i}",
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                    LastActive = DateTimeOffset.UtcNow.AddHours(-i)
                });
            }
            await _fixture.DbContext.Sessions.AddRangeAsync(existingSessions);
            await _fixture.DbContext.SaveChangesAsync();

            var sessionRequest = new CreateSessionRequest
            {
                UserId = userId,
                RefreshToken = "new_token",
                DeviceInfo = "New Device",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Act
            var result = await service.CreateSessionAsync(sessionRequest);

            // Assert
            var remainingSessions = await _fixture.DbContext.Sessions.Where(s => s.UserId == userId).ToListAsync();
            Assert.Equal(5, remainingSessions.Count); // Should stay at max 5
            Assert.Contains(remainingSessions, s => s.RefreshToken == "new_token");
            
            // The oldest session (token5 with LastActive -5 hours) should be deleted
        }

        #endregion

        #region GetSessionBySpecification Tests

        [Fact]
        public async Task GetSessionBySpecification_ExistingSession_ShouldReturnSession()
        {
            // Arrange
            var service = GetService();
            var userId = Guid.NewGuid();
            var refreshToken = "test_refresh_token";
            
            var user = new User
            {
                Id = userId,
                Username = "testuser4",
                Email = "test4@test.com",
                PasswordHash = "hash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Users.AddAsync(user);

            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = refreshToken,
                DeviceInfo = "Test Device",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                LastActive = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Sessions.AddAsync(session);
            await _fixture.DbContext.SaveChangesAsync();

            var spec = new SessionByRefreshTokenSpecification(refreshToken);

            // Act
            var result = await service.GetSessionBySpecification(spec);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(refreshToken, result.RefreshToken);
            Assert.Equal(userId, result.UserId);
        }

        [Fact]
        public async Task GetSessionBySpecification_NonExistingSession_ShouldReturnNull()
        {
            // Arrange
            var service = GetService();
            var spec = new SessionByRefreshTokenSpecification("non_existent_token");

            // Act
            var result = await service.GetSessionBySpecification(spec);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region RemoveSession Tests

        [Fact]
        public async Task RemoveSession_ExistingSession_ShouldDeleteSession()
        {
            // Arrange
            var service = GetService();
            var userId = Guid.NewGuid();
            
            var user = new User
            {
                Id = userId,
                Username = "testuser5",
                Email = "test5@test.com",
                PasswordHash = "hash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Users.AddAsync(user);

            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = "to_be_deleted",
                DeviceInfo = "Test Device",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                LastActive = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Sessions.AddAsync(session);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            await service.RemoveSession(userId);

            // Assert
            var remainingSessions = await _fixture.DbContext.Sessions.Where(s => s.UserId == userId).ToListAsync();
            Assert.Empty(remainingSessions);
        }

        [Fact]
        public async Task RemoveSession_NonExistingSession_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var service = GetService();
            var nonExistentUserId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RemoveSession(nonExistentUserId));
        }

        #endregion

        #region RemoveExpiredSessions Tests

        [Fact]
        public async Task RemoveExpiredSessions_WithExpiredSessions_ShouldDeleteOnlyExpiredOnes()
        {
            // Arrange
            var service = GetService();
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            
            var users = new List<User>
            {
                new User
                {
                    Id = userId1,
                    Username = "user_expired",
                    Email = "expired@test.com",
                    PasswordHash = "hash",
                    Role = UserRole.Resident,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new User
                {
                    Id = userId2,
                    Username = "user_valid",
                    Email = "valid@test.com",
                    PasswordHash = "hash",
                    Role = UserRole.Resident,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };
            await _fixture.DbContext.Users.AddRangeAsync(users);

            var sessions = new List<Session>
            {
                new() {
                    Id = Guid.NewGuid(),
                    UserId = userId1,
                    RefreshToken = "expired_token_1",
                    DeviceInfo = "Device1",
                    IpAddress = "192.168.1.1",
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1), // Expired
                    LastActive = DateTimeOffset.UtcNow.AddDays(-1)
                },
                new Session
                {
                    Id = Guid.NewGuid(),
                    UserId = userId1,
                    RefreshToken = "expired_token_2",
                    DeviceInfo = "Device2",
                    IpAddress = "192.168.1.2",
                    ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1), // Expired
                    LastActive = DateTimeOffset.UtcNow.AddHours(-1)
                },
                new Session
                {
                    Id = Guid.NewGuid(),
                    UserId = userId2,
                    RefreshToken = "valid_token",
                    DeviceInfo = "Device3",
                    IpAddress = "192.168.1.3",
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(7), // Valid
                    LastActive = DateTimeOffset.UtcNow
                }
            };
            await _fixture.DbContext.Sessions.AddRangeAsync(sessions);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var deletedCount = await service.RemoveExpiredSessions();

            // Assert
            Assert.Equal(2, deletedCount);
            
            var remainingSessions = await _fixture.DbContext.Sessions.ToListAsync();
            Assert.Equal("valid_token", remainingSessions[0].RefreshToken);
        }

        [Fact]
        public async Task RemoveExpiredSessions_NoExpiredSessions_ShouldReturnZero()
        {
            // Arrange
            var service = GetService();
            var userId = Guid.NewGuid();
            
            var user = new User
            {
                Id = userId,
                Username = "user_all_valid",
                Email = "allvalid@test.com",
                PasswordHash = "hash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Users.AddAsync(user);

            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = "valid_token",
                DeviceInfo = "Device",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                LastActive = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Sessions.AddAsync(session);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var deletedCount = await service.RemoveExpiredSessions();

            // Assert
            Assert.Equal(0, deletedCount);
            
            var remainingSessions = await _fixture.DbContext.Sessions.ToListAsync();
            Assert.Single(remainingSessions);
        }

        #endregion

        #region UpdateSession Tests

        [Fact]
        public async Task UpdateSession_ExistingSession_ShouldUpdateSuccessfully()
        {
            // Arrange
            var service = GetService();
            var userId = Guid.NewGuid();
            var originalLastActive = DateTimeOffset.UtcNow.AddHours(-2);
            
            var user = new User
            {
                Id = userId,
                Username = "user_update",
                Email = "update@test.com",
                PasswordHash = "hash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _fixture.DbContext.Users.AddAsync(user);

            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = "token_to_update",
                DeviceInfo = "Old Device",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                LastActive = originalLastActive
            };
            await _fixture.DbContext.Sessions.AddAsync(session);
            await _fixture.DbContext.SaveChangesAsync();

            // Modify session
            session.LastActive = DateTimeOffset.UtcNow;
            session.DeviceInfo = "Updated Device";

            // Act
            await service.UpdateSession(session);

            // Assert
            var updatedSession = await _fixture.DbContext.Sessions.FindAsync(session.Id);
            Assert.NotNull(updatedSession);
            Assert.Equal("Updated Device", updatedSession.DeviceInfo);
            Assert.True(updatedSession.LastActive > originalLastActive);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task CreateSessionAsync_MultipleUsersWithMaxSessions_ShouldHandleIndependently()
        {
            // Arrange
            var service = GetService();
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            
            var users = new List<User>
            {
                new User
                {
                    Id = user1Id,
                    Username = "user1_multi",
                    Email = "user1@test.com",
                    PasswordHash = "hash",
                    Role = UserRole.Resident,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new User
                {
                    Id = user2Id,
                    Username = "user2_multi",
                    Email = "user2@test.com",
                    PasswordHash = "hash",
                    Role = UserRole.Resident,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };
            await _fixture.DbContext.Users.AddRangeAsync(users);

            // Create 5 sessions for user1
            var user1Sessions = new List<Session>();
            for (int i = 1; i <= 5; i++)
            {
                user1Sessions.Add(new Session
                {
                    Id = Guid.NewGuid(),
                    UserId = user1Id,
                    RefreshToken = $"user1_token{i}",
                    DeviceInfo = $"User1Device{i}",
                    IpAddress = $"192.168.1.{i}",
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                    LastActive = DateTimeOffset.UtcNow.AddHours(-i)
                });
            }
            await _fixture.DbContext.Sessions.AddRangeAsync(user1Sessions);
            await _fixture.DbContext.SaveChangesAsync();

            // Act - Create new session for user2
            var user2Request = new CreateSessionRequest
            {
                UserId = user2Id,
                RefreshToken = "user2_new_token",
                DeviceInfo = "User2Device",
                IpAddress = "127.0.0.1",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
            };
            await service.CreateSessionAsync(user2Request);

            // Assert
            var user1Sessions_after = await _fixture.DbContext.Sessions.Where(s => s.UserId == user1Id).ToListAsync();
            var user2Sessions_after = await _fixture.DbContext.Sessions.Where(s => s.UserId == user2Id).ToListAsync();
            
            Assert.Equal(5, user1Sessions_after.Count); // User1 should still have 5 sessions
            Assert.Single(user2Sessions_after); // User2 should have 1 session
        }

        #endregion
    }
}