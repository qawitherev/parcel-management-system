using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.NotificationPref;
using ParcelManagement.Test.Fixture;
using Xunit;

namespace ParcelManagement.Test.Service
{
    public class NotificationPrefServiceTest(NotificationPrefFixture notificationPrefFixture) :
        IClassFixture<NotificationPrefFixture>
    {
        private readonly NotificationPrefFixture _fixture = notificationPrefFixture;

        #region CreateNotificationPrefAsync Tests

        [Fact]
        public async Task CreateNotificationPrefAsync_ValidData_ShouldCreateNotificationPref()
        {
            var dbContext = _fixture.DbContext;
            var userId = Guid.NewGuid();
            var creatingUserId = Guid.NewGuid();
            
            // Add user to database
            await dbContext.Users.AddAsync(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash"
            });
            await dbContext.SaveChangesAsync();

            var request = new NotificationPrefCreateRequest
            {
                UserId = userId,
                CreatingUserId = creatingUserId,
                IsEmailActive = true,
                IsWhatsAppActive = false,
                IsOnCheckInActive = true,
                IsOnClaimActive = false,
                IsOverdueActive = true,
                QuietHoursFrom = new TimeOnly(22, 0),
                QuietHoursTo = new TimeOnly(8, 0)
            };

            var result = await _fixture.NpService.CreateNotificationPrefAsync(request);

            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.True(result.IsEmailActive);
            Assert.False(result.IsWhatsAppActive);
            Assert.True(result.IsOnCheckInActive);
            Assert.False(result.IsOnClaimActive);
            Assert.True(result.IsOverdueActive);
            Assert.Equal(new TimeOnly(22, 0), result.QuietHoursFrom);
            Assert.Equal(new TimeOnly(8, 0), result.QuietHoursTo);
            Assert.Equal(creatingUserId, result.CreatedBy);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.NotEqual(DateTimeOffset.MinValue, result.CreatedOn);

            await dbContext.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task CreateNotificationPrefAsync_DefaultValues_ShouldUseDefaults()
        {
            var dbContext = _fixture.DbContext;
            var userId = Guid.NewGuid();
            var creatingUserId = Guid.NewGuid();
            
            await dbContext.Users.AddAsync(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash"
            });
            await dbContext.SaveChangesAsync();

            var request = new NotificationPrefCreateRequest
            {
                UserId = userId,
                CreatingUserId = creatingUserId
            };

            var result = await _fixture.NpService.CreateNotificationPrefAsync(request);

            Assert.NotNull(result);
            Assert.True(result.IsEmailActive);
            Assert.True(result.IsWhatsAppActive);
            Assert.True(result.IsOnCheckInActive);
            Assert.False(result.IsOnClaimActive);
            Assert.True(result.IsOverdueActive);
            Assert.Null(result.QuietHoursFrom);
            Assert.Null(result.QuietHoursTo);

            await dbContext.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task CreateNotificationPrefAsync_UserAlreadyHasPreferences_ShouldThrowError()
        {
            var dbContext = _fixture.DbContext;
            var userId = Guid.NewGuid();
            var creatingUserId = Guid.NewGuid();

            await dbContext.Users.AddAsync(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash"
            });

            // Add existing notification preferences
            await dbContext.NotificationPref.AddAsync(new NotificationPref
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedBy = creatingUserId,
                CreatedOn = DateTimeOffset.UtcNow
            });
            await dbContext.SaveChangesAsync();

            var request = new NotificationPrefCreateRequest
            {
                UserId = userId,
                CreatingUserId = creatingUserId
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _fixture.NpService.CreateNotificationPrefAsync(request);
            });

            await dbContext.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task CreateNotificationPrefAsync_WithQuietHours_ShouldSaveQuietHours()
        {
            var dbContext = _fixture.DbContext;
            var userId = Guid.NewGuid();
            var creatingUserId = Guid.NewGuid();
            
            await dbContext.Users.AddAsync(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash"
            });
            await dbContext.SaveChangesAsync();

            var request = new NotificationPrefCreateRequest
            {
                UserId = userId,
                CreatingUserId = creatingUserId,
                QuietHoursFrom = new TimeOnly(23, 30),
                QuietHoursTo = new TimeOnly(7, 0)
            };

            var result = await _fixture.NpService.CreateNotificationPrefAsync(request);

            Assert.NotNull(result);
            Assert.NotNull(result.QuietHoursFrom);
            Assert.NotNull(result.QuietHoursTo);
            Assert.Equal(new TimeOnly(23, 30), result.QuietHoursFrom);
            Assert.Equal(new TimeOnly(7, 0), result.QuietHoursTo);

            await dbContext.Database.EnsureDeletedAsync();
        }

        #endregion

        #region GetNotificationPrefByIdAsync Tests

        [Fact]
        public async Task GetNotificationPrefByIdAsync_ExistingId_ShouldReturnNotificationPref()
        {
            var dbContext = _fixture.DbContext;
            var userId = Guid.NewGuid();
            var notificationPrefId = Guid.NewGuid();

            await dbContext.Users.AddAsync(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash"
            });

            var notificationPref = new NotificationPref
            {
                Id = notificationPrefId,
                UserId = userId,
                IsEmailActive = true,
                IsWhatsAppActive = false,
                IsOnCheckInActive = true,
                IsOnClaimActive = false,
                IsOverdueActive = true,
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            };
            await dbContext.NotificationPref.AddAsync(notificationPref);
            await dbContext.SaveChangesAsync();

            var result = await _fixture.NpService.GetNotificationPrefByIdAsync(notificationPrefId);

            Assert.NotNull(result);
            Assert.Equal(notificationPrefId, result.Id);
            Assert.Equal(userId, result.UserId);
            Assert.True(result.IsEmailActive);
            Assert.False(result.IsWhatsAppActive);

            await dbContext.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task GetNotificationPrefByIdAsync_NonExistentId_ShouldThrowError()
        {
            var nonExistentId = Guid.NewGuid();

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _fixture.NpService.GetNotificationPrefByIdAsync(nonExistentId);
            });

            await _fixture.DbContext.Database.EnsureDeletedAsync();
        }

        #endregion

        #region GetNotificationPrefByUserId Tests

        [Fact]
        public async Task GetNotificationPrefByUserId_ExistingUser_ShouldReturnNotificationPref()
        {
            var dbContext = _fixture.DbContext;
            var userId = Guid.NewGuid();
            var notificationPrefId = Guid.NewGuid();

            await dbContext.Users.AddAsync(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash"
            });

            var notificationPref = new NotificationPref
            {
                Id = notificationPrefId,
                UserId = userId,
                IsEmailActive = true,
                IsWhatsAppActive = true,
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            };
            await dbContext.NotificationPref.AddAsync(notificationPref);
            await dbContext.SaveChangesAsync();

            var result = await _fixture.NpService.GetNotificationPrefByUserId(userId);

            Assert.NotNull(result);
            Assert.Equal(notificationPrefId, result.Id);
            Assert.Equal(userId, result.UserId);

            await dbContext.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task GetNotificationPrefByUserId_UserWithoutPreferences_ShouldReturnNull()
        {
            var dbContext = _fixture.DbContext;
            var userId = Guid.NewGuid();

            await dbContext.Users.AddAsync(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash"
            });
            await dbContext.SaveChangesAsync();

            var result = await _fixture.NpService.GetNotificationPrefByUserId(userId);

            Assert.Null(result);

            await dbContext.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task GetNotificationPrefByUserId_NonExistentUser_ShouldThrowError()
        {
            var nonExistentUserId = Guid.NewGuid();

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _fixture.NpService.GetNotificationPrefByUserId(nonExistentUserId);
            });

            await _fixture.DbContext.Database.EnsureDeletedAsync();
        }

        #endregion

        #region UpdateNotificationPrefs Tests

        [Fact]
        public async Task UpdateNotificationPrefs_ValidUpdate_ShouldUpdateNotificationPref()
        {
            var dbContext = _fixture.DbContext;
            var userId = Guid.NewGuid();
            var notificationPrefId = Guid.NewGuid();

            await dbContext.Users.AddAsync(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash"
            });

            var notificationPref = new NotificationPref
            {
                Id = notificationPrefId,
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
            await dbContext.NotificationPref.AddAsync(notificationPref);
            await dbContext.SaveChangesAsync();

            var updateRequest = new NotificationPrefUpdateRequest
            {
                NotificationPrefId = notificationPrefId,
                UserId = userId,
                UpdatingUserId = userId,
                IsEmailActive = false,
                IsWhatsAppActive = false,
                IsOnCheckInActive = false,
                IsOnClaimActive = true,
                IsOverdueActive = false,
                QuietHoursFrom = new TimeOnly(22, 0),
                QuietHoursTo = new TimeOnly(8, 0)
            };

            await _fixture.NpService.UpdateNotificationPrefs(updateRequest, userId);

            var updated = await dbContext.NotificationPref.FindAsync(notificationPrefId);
            await dbContext.Entry(updated!).ReloadAsync();

            Assert.NotNull(updated);
            Assert.False(updated.IsEmailActive);
            Assert.False(updated.IsWhatsAppActive);
            Assert.False(updated.IsOnCheckInActive);
            Assert.True(updated.IsOnClaimActive);
            Assert.False(updated.IsOverdueActive);
            Assert.Equal(new TimeOnly(22, 0), updated.QuietHoursFrom);
            Assert.Equal(new TimeOnly(8, 0), updated.QuietHoursTo);
            Assert.Equal(userId, updated.UpdatedBy);
            Assert.NotNull(updated.UpdatedOn);

            await dbContext.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task UpdateNotificationPrefs_PartialUpdate_ShouldUpdateOnlyProvidedFields()
        {
            var dbContext = _fixture.DbContext;
            var userId = Guid.NewGuid();
            var notificationPrefId = Guid.NewGuid();

            await dbContext.Users.AddAsync(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash"
            });

            var notificationPref = new NotificationPref
            {
                Id = notificationPrefId,
                UserId = userId,
                IsEmailActive = true,
                IsWhatsAppActive = true,
                IsOnCheckInActive = true,
                IsOnClaimActive = false,
                IsOverdueActive = true,
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            };
            await dbContext.NotificationPref.AddAsync(notificationPref);
            await dbContext.SaveChangesAsync();

            var updateRequest = new NotificationPrefUpdateRequest
            {
                NotificationPrefId = notificationPrefId,
                UserId = userId,
                UpdatingUserId = userId,
                IsEmailActive = false,
                // Other fields are null, should keep existing values
                IsWhatsAppActive = null,
                IsOnCheckInActive = null,
                IsOnClaimActive = null,
                IsOverdueActive = null
            };

            await _fixture.NpService.UpdateNotificationPrefs(updateRequest, userId);

            var updated = await dbContext.NotificationPref.FindAsync(notificationPrefId);
            await dbContext.Entry(updated!).ReloadAsync();

            Assert.NotNull(updated);
            Assert.False(updated.IsEmailActive); // Updated
            Assert.True(updated.IsWhatsAppActive); // Unchanged
            Assert.True(updated.IsOnCheckInActive); // Unchanged
            Assert.False(updated.IsOnClaimActive); // Unchanged
            Assert.True(updated.IsOverdueActive); // Unchanged

            await dbContext.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task UpdateNotificationPrefs_NonExistentId_ShouldThrowError()
        {
            var userId = Guid.NewGuid();
            var nonExistentId = Guid.NewGuid();

            var updateRequest = new NotificationPrefUpdateRequest
            {
                NotificationPrefId = nonExistentId,
                UserId = userId,
                UpdatingUserId = userId,
                IsEmailActive = false
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _fixture.NpService.UpdateNotificationPrefs(updateRequest, userId);
            });

            await _fixture.DbContext.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task UpdateNotificationPrefs_DifferentUser_ShouldThrowError()
        {
            var dbContext = _fixture.DbContext;
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var notificationPrefId = Guid.NewGuid();

            await dbContext.Users.AddAsync(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash"
            });
            await dbContext.Users.AddAsync(new User
            {
                Id = otherUserId,
                Username = "otheruser",
                Email = "other@example.com",
                PasswordHash = "hash"
            });

            var notificationPref = new NotificationPref
            {
                Id = notificationPrefId,
                UserId = userId,
                IsEmailActive = true,
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            };
            await dbContext.NotificationPref.AddAsync(notificationPref);
            await dbContext.SaveChangesAsync();

            var updateRequest = new NotificationPrefUpdateRequest
            {
                NotificationPrefId = notificationPrefId,
                UserId = otherUserId,
                UpdatingUserId = otherUserId,
                IsEmailActive = false
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _fixture.NpService.UpdateNotificationPrefs(updateRequest, otherUserId);
            });

            await dbContext.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task UpdateNotificationPrefs_UpdateQuietHours_ShouldUpdateQuietHours()
        {
            var dbContext = _fixture.DbContext;
            var userId = Guid.NewGuid();
            var notificationPrefId = Guid.NewGuid();

            await dbContext.Users.AddAsync(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash"
            });

            var notificationPref = new NotificationPref
            {
                Id = notificationPrefId,
                UserId = userId,
                IsEmailActive = true,
                QuietHoursFrom = new TimeOnly(22, 0),
                QuietHoursTo = new TimeOnly(8, 0),
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            };
            await dbContext.NotificationPref.AddAsync(notificationPref);
            await dbContext.SaveChangesAsync();

            var updateRequest = new NotificationPrefUpdateRequest
            {
                NotificationPrefId = notificationPrefId,
                UserId = userId,
                UpdatingUserId = userId,
                QuietHoursFrom = new TimeOnly(23, 0),
                QuietHoursTo = new TimeOnly(7, 30)
            };

            await _fixture.NpService.UpdateNotificationPrefs(updateRequest, userId);

            var updated = await dbContext.NotificationPref.FindAsync(notificationPrefId);
            await dbContext.Entry(updated!).ReloadAsync();

            Assert.NotNull(updated);
            Assert.Equal(new TimeOnly(23, 0), updated.QuietHoursFrom);
            Assert.Equal(new TimeOnly(7, 30), updated.QuietHoursTo);

            await dbContext.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task UpdateNotificationPrefs_ClearQuietHours_ShouldSetToNull()
        {
            var dbContext = _fixture.DbContext;
            var userId = Guid.NewGuid();
            var notificationPrefId = Guid.NewGuid();

            await dbContext.Users.AddAsync(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash"
            });

            var notificationPref = new NotificationPref
            {
                Id = notificationPrefId,
                UserId = userId,
                IsEmailActive = true,
                QuietHoursFrom = new TimeOnly(22, 0),
                QuietHoursTo = new TimeOnly(8, 0),
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            };
            await dbContext.NotificationPref.AddAsync(notificationPref);
            await dbContext.SaveChangesAsync();

            var updateRequest = new NotificationPrefUpdateRequest
            {
                NotificationPrefId = notificationPrefId,
                UserId = userId,
                UpdatingUserId = userId,
                QuietHoursFrom = null,
                QuietHoursTo = null
            };

            await _fixture.NpService.UpdateNotificationPrefs(updateRequest, userId);

            var updated = await dbContext.NotificationPref.FindAsync(notificationPrefId);
            await dbContext.Entry(updated!).ReloadAsync();

            Assert.NotNull(updated);
            Assert.Null(updated.QuietHoursFrom);
            Assert.Null(updated.QuietHoursTo);

            await dbContext.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task UpdateNotificationPrefs_AllNotificationsDisabled_ShouldAllowUpdate()
        {
            var dbContext = _fixture.DbContext;
            var userId = Guid.NewGuid();
            var notificationPrefId = Guid.NewGuid();

            await dbContext.Users.AddAsync(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash"
            });

            var notificationPref = new NotificationPref
            {
                Id = notificationPrefId,
                UserId = userId,
                IsEmailActive = true,
                IsWhatsAppActive = true,
                IsOnCheckInActive = true,
                IsOnClaimActive = true,
                IsOverdueActive = true,
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            };
            await dbContext.NotificationPref.AddAsync(notificationPref);
            await dbContext.SaveChangesAsync();

            var updateRequest = new NotificationPrefUpdateRequest
            {
                NotificationPrefId = notificationPrefId,
                UserId = userId,
                UpdatingUserId = userId,
                IsEmailActive = false,
                IsWhatsAppActive = false,
                IsOnCheckInActive = false,
                IsOnClaimActive = false,
                IsOverdueActive = false
            };

            await _fixture.NpService.UpdateNotificationPrefs(updateRequest, userId);

            var updated = await dbContext.NotificationPref.FindAsync(notificationPrefId);
            await dbContext.Entry(updated!).ReloadAsync();

            Assert.NotNull(updated);
            Assert.False(updated.IsEmailActive);
            Assert.False(updated.IsWhatsAppActive);
            Assert.False(updated.IsOnCheckInActive);
            Assert.False(updated.IsOnClaimActive);
            Assert.False(updated.IsOverdueActive);

            await dbContext.Database.EnsureDeletedAsync();
        }

        #endregion
    }
}
