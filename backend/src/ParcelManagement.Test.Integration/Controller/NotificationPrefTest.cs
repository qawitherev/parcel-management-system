using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ParcelManagement.Api.AuthenticationAndAuthorization;
using ParcelManagement.Api.DTO.V1;
using ParcelManagement.Core.Entities;
using ParcelManagement.Test.Integration.Misc;

namespace ParcelManagement.Test.Integration
{
    public class NotificationPrefControllerTest : IntegrationTestBase
    {
        private readonly CustomWebApplicationFactory _factory;

        public NotificationPrefControllerTest(CustomWebApplicationFactory factory) 
            : base(factory, factory.Services.GetRequiredService<ITokenService>())
        {
            _factory = factory;
        }

        #region CreateNotificationPref Tests

        [Fact]
        public async Task CreateNotificationPref_ValidData_ShouldReturn201()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var payload = new NotificationPrefCreateRequestDto
            {
                IsEmailActive = true,
                IsWhatsAppActive = false,
                IsOnCheckInActive = true,
                IsOnClaimActive = false,
                IsOverdueActive = true,
                QuietHoursFrom = new TimeOnly(22, 0),
                QuietHoursTo = new TimeOnly(8, 0)
            };

            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/NotificationPref", body);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdPref = await response.Content.ReadFromJsonAsync<NotificationPref>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(createdPref);
            Assert.Equal(userId, createdPref.UserId);
            Assert.True(createdPref.IsEmailActive);
            Assert.False(createdPref.IsWhatsAppActive);
            Assert.True(createdPref.IsOnCheckInActive);
            Assert.False(createdPref.IsOnClaimActive);
            Assert.True(createdPref.IsOverdueActive);
            Assert.Equal(new TimeOnly(22, 0), createdPref.QuietHoursFrom);
            Assert.Equal(new TimeOnly(8, 0), createdPref.QuietHoursTo);
        }

        [Fact]
        public async Task CreateNotificationPref_DefaultValues_ShouldReturn201WithDefaults()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var payload = new NotificationPrefCreateRequestDto();

            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/NotificationPref", body);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdPref = await response.Content.ReadFromJsonAsync<NotificationPref>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(createdPref);
            Assert.True(createdPref.IsEmailActive);
            Assert.True(createdPref.IsWhatsAppActive);
            Assert.True(createdPref.IsOnCheckInActive);
            Assert.False(createdPref.IsOnClaimActive);
            Assert.True(createdPref.IsOverdueActive);
        }

        [Fact]
        public async Task CreateNotificationPref_AlreadyExists_ShouldReturn500()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create notification preferences first
            await DbContext.NotificationPref.AddAsync(new NotificationPref
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            });
            await DbContext.SaveChangesAsync();

            var payload = new NotificationPrefCreateRequestDto();
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/NotificationPref", body);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task CreateNotificationPref_WithoutAuth_ShouldReturn401()
        {
            await ResetDatabaseAsync();

            var payload = new NotificationPrefCreateRequestDto();
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/NotificationPref", body);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateNotificationPref_AsNonResident_ShouldReturn403()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ManagerTest", "ParcelRoomManager");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var payload = new NotificationPrefCreateRequestDto();
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/NotificationPref", body);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateNotificationPref_WithQuietHours_ShouldReturn201()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var payload = new NotificationPrefCreateRequestDto
            {
                QuietHoursFrom = new TimeOnly(23, 30),
                QuietHoursTo = new TimeOnly(7, 0)
            };

            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/NotificationPref", body);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdPref = await response.Content.ReadFromJsonAsync<NotificationPref>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(createdPref);
            Assert.Equal(new TimeOnly(23, 30), createdPref.QuietHoursFrom);
            Assert.Equal(new TimeOnly(7, 0), createdPref.QuietHoursTo);
        }

        #endregion

        #region GetNotificationPrefById Tests

        [Fact]
        public async Task GetNotificationPrefById_ExistingPref_ShouldReturn200()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            var notificationPrefId = Guid.NewGuid();

            await DbContext.NotificationPref.AddAsync(new NotificationPref
            {
                Id = notificationPrefId,
                UserId = userId,
                IsEmailActive = true,
                IsWhatsAppActive = false,
                IsOnCheckInActive = true,
                IsOnClaimActive = false,
                IsOverdueActive = true,
                QuietHoursFrom = new TimeOnly(22, 0),
                QuietHoursTo = new TimeOnly(8, 0),
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            });
            await DbContext.SaveChangesAsync();

            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await Client.GetAsync($"api/v1/NotificationPref/{notificationPrefId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var fetchedPref = await response.Content.ReadFromJsonAsync<NotificationPrefResponseDto>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(fetchedPref);
            Assert.Equal(notificationPrefId, fetchedPref.Id);
            Assert.Equal(userId, fetchedPref.UserId);
            Assert.True(fetchedPref.IsEmailActive);
            Assert.False(fetchedPref.IsWhatsAppActive);
            Assert.Equal(new TimeOnly(22, 0), fetchedPref.QuietHoursFrom);
            Assert.Equal(new TimeOnly(8, 0), fetchedPref.QuietHoursTo);
        }

        [Fact]
        public async Task GetNotificationPrefById_NonExistentPref_ShouldReturn404()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var nonExistentId = Guid.NewGuid();
            var response = await Client.GetAsync($"api/v1/NotificationPref/{nonExistentId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetNotificationPrefById_WithoutAuth_ShouldReturn401()
        {
            await ResetDatabaseAsync();
            var notificationPrefId = Guid.NewGuid();

            var response = await Client.GetAsync($"api/v1/NotificationPref/{notificationPrefId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetNotificationPrefById_AsNonResident_ShouldReturn403()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ManagerTest", "ParcelRoomManager");
            var notificationPrefId = Guid.NewGuid();

            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await Client.GetAsync($"api/v1/NotificationPref/{notificationPrefId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        #endregion

        #region GetNotificationPrefByUser Tests

        [Fact]
        public async Task GetNotificationPrefByUser_ExistingPref_ShouldReturn200()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            var notificationPrefId = Guid.NewGuid();

            await DbContext.NotificationPref.AddAsync(new NotificationPref
            {
                Id = notificationPrefId,
                UserId = userId,
                IsEmailActive = true,
                IsWhatsAppActive = true,
                IsOnCheckInActive = false,
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            });
            await DbContext.SaveChangesAsync();

            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await Client.GetAsync("api/v1/NotificationPref/me");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var fetchedPref = await response.Content.ReadFromJsonAsync<NotificationPrefResponseDto>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(fetchedPref);
            Assert.Equal(notificationPrefId, fetchedPref.Id);
            Assert.Equal(userId, fetchedPref.UserId);
            Assert.True(fetchedPref.IsEmailActive);
            Assert.True(fetchedPref.IsWhatsAppActive);
            Assert.False(fetchedPref.IsOnCheckInActive);
        }

        [Fact]
        public async Task GetNotificationPrefByUser_NoPref_ShouldReturn500()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await Client.GetAsync("api/v1/NotificationPref/me");

            // Service returns null which causes NullReferenceException when mapping to DTO
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetNotificationPrefByUser_WithoutAuth_ShouldReturn401()
        {
            await ResetDatabaseAsync();

            var response = await Client.GetAsync("api/v1/NotificationPref/me");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetNotificationPrefByUser_AsNonResident_ShouldReturn403()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ManagerTest", "ParcelRoomManager");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await Client.GetAsync("api/v1/NotificationPref/me");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        #endregion

        #region UpdateNotificationPref Tests

        [Fact]
        public async Task UpdateNotificationPref_ValidUpdate_ShouldReturn204()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            var notificationPrefId = Guid.NewGuid();

            await DbContext.NotificationPref.AddAsync(new NotificationPref
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
            });
            await DbContext.SaveChangesAsync();

            var payload = new NotificationPrefUpdateRequestDto
            {
                IsEmailActive = false,
                IsWhatsAppActive = false,
                IsOnCheckInActive = false,
                IsOnClaimActive = true,
                IsOverdueActive = false,
                QuietHoursFrom = new TimeOnly(23, 0),
                QuietHoursTo = new TimeOnly(7, 30)
            };

            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PatchAsync($"api/v1/NotificationPref/{notificationPrefId}", body);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the update
            ClearChangeTracker();
            var updated = await DbContext.NotificationPref.FindAsync(notificationPrefId);
            Assert.NotNull(updated);
            Assert.False(updated.IsEmailActive);
            Assert.False(updated.IsWhatsAppActive);
            Assert.False(updated.IsOnCheckInActive);
            Assert.True(updated.IsOnClaimActive);
            Assert.False(updated.IsOverdueActive);
            Assert.Equal(new TimeOnly(23, 0), updated.QuietHoursFrom);
            Assert.Equal(new TimeOnly(7, 30), updated.QuietHoursTo);
        }

        [Fact]
        public async Task UpdateNotificationPref_PartialUpdate_ShouldReturn204()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            var notificationPrefId = Guid.NewGuid();

            await DbContext.NotificationPref.AddAsync(new NotificationPref
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
            });
            await DbContext.SaveChangesAsync();

            var payload = new NotificationPrefUpdateRequestDto
            {
                IsEmailActive = false,
                // Other fields are null, should keep existing values
            };

            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PatchAsync($"api/v1/NotificationPref/{notificationPrefId}", body);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify only IsEmailActive was updated
            ClearChangeTracker();
            var updated = await DbContext.NotificationPref.FindAsync(notificationPrefId);
            Assert.NotNull(updated);
            Assert.False(updated.IsEmailActive); // Updated
            Assert.True(updated.IsWhatsAppActive); // Unchanged
            Assert.True(updated.IsOnCheckInActive); // Unchanged
            Assert.False(updated.IsOnClaimActive); // Unchanged
            Assert.True(updated.IsOverdueActive); // Unchanged
        }

        [Fact]
        public async Task UpdateNotificationPref_NonExistentPref_ShouldReturn404()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            var nonExistentId = Guid.NewGuid();

            var payload = new NotificationPrefUpdateRequestDto
            {
                IsEmailActive = false
            };

            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PatchAsync($"api/v1/NotificationPref/{nonExistentId}", body);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNotificationPref_DifferentUser_ShouldReturn409()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            var notificationPrefId = Guid.NewGuid();

            // Create user and their preferences
            var otherUser = new User
            {
                Id = otherUserId,
                Username = "OtherUser",
                Email = "other@test.com",
                PasswordHash = "########",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await DbContext.Users.AddAsync(otherUser);

            await DbContext.NotificationPref.AddAsync(new NotificationPref
            {
                Id = notificationPrefId,
                UserId = otherUserId, // Different user
                IsEmailActive = true,
                CreatedBy = otherUserId,
                CreatedOn = DateTimeOffset.UtcNow
            });
            await DbContext.SaveChangesAsync();

            var payload = new NotificationPrefUpdateRequestDto
            {
                IsEmailActive = false
            };

            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PatchAsync($"api/v1/NotificationPref/{notificationPrefId}", body);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNotificationPref_WithoutAuth_ShouldReturn401()
        {
            await ResetDatabaseAsync();
            var notificationPrefId = Guid.NewGuid();

            var payload = new NotificationPrefUpdateRequestDto
            {
                IsEmailActive = false
            };

            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PatchAsync($"api/v1/NotificationPref/{notificationPrefId}", body);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNotificationPref_AsNonResident_ShouldReturn403()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ManagerTest", "ParcelRoomManager");
            var notificationPrefId = Guid.NewGuid();

            var payload = new NotificationPrefUpdateRequestDto
            {
                IsEmailActive = false
            };

            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PatchAsync($"api/v1/NotificationPref/{notificationPrefId}", body);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNotificationPref_ClearQuietHours_ShouldReturn204()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            var notificationPrefId = Guid.NewGuid();

            await DbContext.NotificationPref.AddAsync(new NotificationPref
            {
                Id = notificationPrefId,
                UserId = userId,
                IsEmailActive = true,
                QuietHoursFrom = new TimeOnly(22, 0),
                QuietHoursTo = new TimeOnly(8, 0),
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            });
            await DbContext.SaveChangesAsync();

            var payload = new NotificationPrefUpdateRequestDto
            {
                QuietHoursFrom = null,
                QuietHoursTo = null
            };

            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PatchAsync($"api/v1/NotificationPref/{notificationPrefId}", body);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify quiet hours were cleared
            ClearChangeTracker();
            var updated = await DbContext.NotificationPref.FindAsync(notificationPrefId);
            Assert.NotNull(updated);
            Assert.Null(updated.QuietHoursFrom);
            Assert.Null(updated.QuietHoursTo);
        }

        [Fact]
        public async Task UpdateNotificationPref_DisableAllNotifications_ShouldReturn204()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            var notificationPrefId = Guid.NewGuid();

            await DbContext.NotificationPref.AddAsync(new NotificationPref
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
            });
            await DbContext.SaveChangesAsync();

            var payload = new NotificationPrefUpdateRequestDto
            {
                IsEmailActive = false,
                IsWhatsAppActive = false,
                IsOnCheckInActive = false,
                IsOnClaimActive = false,
                IsOverdueActive = false
            };

            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PatchAsync($"api/v1/NotificationPref/{notificationPrefId}", body);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify all notifications are disabled
            ClearChangeTracker();
            var updated = await DbContext.NotificationPref.FindAsync(notificationPrefId);
            Assert.NotNull(updated);
            Assert.False(updated.IsEmailActive);
            Assert.False(updated.IsWhatsAppActive);
            Assert.False(updated.IsOnCheckInActive);
            Assert.False(updated.IsOnClaimActive);
            Assert.False(updated.IsOverdueActive);
        }

        [Fact]
        public async Task UpdateNotificationPref_SetQuietHours_ShouldReturn204()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            var notificationPrefId = Guid.NewGuid();

            await DbContext.NotificationPref.AddAsync(new NotificationPref
            {
                Id = notificationPrefId,
                UserId = userId,
                IsEmailActive = true,
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            });
            await DbContext.SaveChangesAsync();

            var payload = new NotificationPrefUpdateRequestDto
            {
                QuietHoursFrom = new TimeOnly(23, 30),
                QuietHoursTo = new TimeOnly(6, 30)
            };

            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PatchAsync($"api/v1/NotificationPref/{notificationPrefId}", body);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify quiet hours were set
            ClearChangeTracker();
            var updated = await DbContext.NotificationPref.FindAsync(notificationPrefId);
            Assert.NotNull(updated);
            Assert.Equal(new TimeOnly(23, 30), updated.QuietHoursFrom);
            Assert.Equal(new TimeOnly(6, 30), updated.QuietHoursTo);
        }

        #endregion
    }
}
