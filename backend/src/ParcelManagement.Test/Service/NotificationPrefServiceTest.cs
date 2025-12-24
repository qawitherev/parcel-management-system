using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.NotificationPref;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Test.Fixture;
using Xunit;

namespace ParcelManagement.Test.Service
{
    public class NotificationPrefServiceTest(NotificationPrefFixture fixture) : IClassFixture<NotificationPrefFixture>
    {
        private readonly NotificationPrefFixture _fixture = fixture;

        [Fact]
        public async Task CreateNotificationPrefAsync_UserAlreadyHasPreferences_ShouldThrowError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var creatingUserId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@email.com",
                PasswordHash = "hash",
                Role = UserRole.Resident
            };

            var existingNp = new NotificationPref
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IsEmailActive = true,
                IsWhatsAppActive = true,
                IsOnCheckInActive = true,
                IsOnClaimActive = false,
                IsOverdueActive = true,
                CreatedBy = creatingUserId,
                CreatedOn = DateTimeOffset.UtcNow
            };

            await _fixture.DbContext.Users.AddAsync(user);
            await _fixture.DbContext.NotificationPref.AddAsync(existingNp);
            await _fixture.DbContext.SaveChangesAsync();

            var createRequest = new NotificationPrefCreateRequest
            {
                UserId = userId,
                CreatingUserId = creatingUserId,
                IsEmailActive = false,
                IsWhatsAppActive = false,
                IsOnCheckInActive = true,
                IsOnClaimActive = true,
                IsOverdueActive = true
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _fixture.NpService.CreateNotificationPrefAsync(createRequest);
            });
        }

        [Fact]
        public async Task CreateNotificationPrefAsync_ValidRequest_ShouldCreateNotificationPref()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var creatingUserId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Username = "newuser",
                Email = "newuser@email.com",
                PasswordHash = "hash",
                Role = UserRole.Resident
            };

            await _fixture.DbContext.Users.AddAsync(user);
            await _fixture.DbContext.SaveChangesAsync();

            var createRequest = new NotificationPrefCreateRequest
            {
                UserId = userId,
                CreatingUserId = creatingUserId,
                IsEmailActive = true,
                IsWhatsAppActive = false,
                IsOnCheckInActive = true,
                IsOnClaimActive = true,
                IsOverdueActive = false,
                QuietHoursFrom = DateTimeOffset.UtcNow.AddHours(-2),
                QuietHoursTo = DateTimeOffset.UtcNow.AddHours(6)
            };

            // Act
            var result = await _fixture.NpService.CreateNotificationPrefAsync(createRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.True(result.IsEmailActive);
            Assert.False(result.IsWhatsAppActive);
            Assert.True(result.IsOnCheckInActive);
            Assert.True(result.IsOnClaimActive);
            Assert.False(result.IsOverdueActive);
            Assert.NotNull(result.QuietHoursFrom);
            Assert.NotNull(result.QuietHoursTo);

            // Verify it's actually in the database
            var specification = new NotificationPrefByUserIdSpecification(userId);
            var savedNp = await _fixture.DbContext.NotificationPref
                .Where(specification.ToExpression())
                .FirstOrDefaultAsync();
            Assert.NotNull(savedNp);
            Assert.Equal(userId, savedNp.UserId);
        }

        [Fact]
        public async Task GetNotificationPrefByIdAsync_NotificationPrefNotFound_ShouldThrowError()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _fixture.NpService.GetNotificationPrefByIdAsync(nonExistentId);
            });
        }

        [Fact]
        public async Task GetNotificationPrefByIdAsync_ValidId_ShouldReturnNotificationPref()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var npId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Username = "getbyiduser",
                Email = "getbyid@email.com",
                PasswordHash = "hash",
                Role = UserRole.Resident
            };

            var notificationPref = new NotificationPref
            {
                Id = npId,
                UserId = userId,
                IsEmailActive = true,
                IsWhatsAppActive = true,
                IsOnCheckInActive = false,
                IsOnClaimActive = true,
                IsOverdueActive = true,
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            };

            await _fixture.DbContext.Users.AddAsync(user);
            await _fixture.DbContext.NotificationPref.AddAsync(notificationPref);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.NpService.GetNotificationPrefByIdAsync(npId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(npId, result.Id);
            Assert.Equal(userId, result.UserId);
            Assert.True(result.IsEmailActive);
            Assert.False(result.IsOnCheckInActive);
        }

        [Fact]
        public async Task GetNotificationPrefByUserId_UserNotFound_ShouldThrowError()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _fixture.NpService.GetNotificationPrefByUserId(nonExistentUserId);
            });
        }

        [Fact]
        public async Task GetNotificationPrefByUserId_ValidUserId_ShouldReturnNotificationPref()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var npId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Username = "getbyuseriduser",
                Email = "getbyuserid@email.com",
                PasswordHash = "hash",
                Role = UserRole.Resident
            };

            var notificationPref = new NotificationPref
            {
                Id = npId,
                UserId = userId,
                IsEmailActive = false,
                IsWhatsAppActive = true,
                IsOnCheckInActive = true,
                IsOnClaimActive = false,
                IsOverdueActive = true,
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            };

            await _fixture.DbContext.Users.AddAsync(user);
            await _fixture.DbContext.NotificationPref.AddAsync(notificationPref);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.NpService.GetNotificationPrefByUserId(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(npId, result.Id);
            Assert.Equal(userId, result.UserId);
            Assert.False(result.IsEmailActive);
            Assert.True(result.IsWhatsAppActive);
        }

        [Fact]
        public async Task GetNotificationPrefByUserId_NoPreferencesFound_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Username = "noprefsuser",
                Email = "noprefs@email.com",
                PasswordHash = "hash",
                Role = UserRole.Resident
            };

            await _fixture.DbContext.Users.AddAsync(user);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var result = await _fixture.NpService.GetNotificationPrefByUserId(userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateNotificationPrefs_NotificationPrefNotFound_ShouldThrowError()
        {
            // Arrange
            var updateRequest = new NotificationPrefUpdateRequest
            {
                NotificationPrefId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                UpdatingUserId = Guid.NewGuid(),
                IsEmailActive = true,
                IsWhatsAppActive = true,
                IsOnCheckInActive = true,
                IsOnClaimActive = true,
                IsOverdueActive = true
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _fixture.NpService.UpdateNotificationPrefs(updateRequest);
            });
        }

        [Fact]
        public async Task UpdateNotificationPrefs_ValidRequest_ShouldUpdateNotificationPref()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var npId = Guid.NewGuid();
            var updatingUserId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Username = "updateuser",
                Email = "update@email.com",
                PasswordHash = "hash",
                Role = UserRole.Resident
            };

            var notificationPref = new NotificationPref
            {
                Id = npId,
                UserId = userId,
                IsEmailActive = true,
                IsWhatsAppActive = true,
                IsOnCheckInActive = true,
                IsOnClaimActive = false,
                IsOverdueActive = true,
                QuietHoursFrom = null,
                QuietHoursTo = null,
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            };

            await _fixture.DbContext.Users.AddAsync(user);
            await _fixture.DbContext.NotificationPref.AddAsync(notificationPref);
            await _fixture.DbContext.SaveChangesAsync();

            var updateRequest = new NotificationPrefUpdateRequest
            {
                NotificationPrefId = npId,
                UserId = userId,
                UpdatingUserId = updatingUserId,
                IsEmailActive = false,
                IsWhatsAppActive = false,
                IsOnCheckInActive = false,
                IsOnClaimActive = true,
                IsOverdueActive = false,
                QuietHoursFrom = DateTimeOffset.UtcNow.AddHours(-3),
                QuietHoursTo = DateTimeOffset.UtcNow.AddHours(5)
            };

            // Act
            await _fixture.NpService.UpdateNotificationPrefs(updateRequest);

            // Assert
            var updatedNp = await _fixture.DbContext.NotificationPref.FindAsync(npId);
            Assert.NotNull(updatedNp);
            Assert.False(updatedNp.IsEmailActive);
            Assert.False(updatedNp.IsWhatsAppActive);
            Assert.False(updatedNp.IsOnCheckInActive);
            Assert.True(updatedNp.IsOnClaimActive);
            Assert.False(updatedNp.IsOverdueActive);
            Assert.NotNull(updatedNp.QuietHoursFrom);
            Assert.NotNull(updatedNp.QuietHoursTo);
            Assert.Equal(updatingUserId, updatedNp.UpdatedBy);
            Assert.NotNull(updatedNp.UpdatedOn);
        }
    }
}
