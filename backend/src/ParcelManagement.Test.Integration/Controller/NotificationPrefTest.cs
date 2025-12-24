using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ParcelManagement.Api.AuthenticationAndAuthorization;
using ParcelManagement.Api.DTO.V1;
using ParcelManagement.Core.Entities;
using ParcelManagement.Test.Integration.Misc;
using Xunit;

namespace ParcelManagement.Test.Integration
{
    public class NotificationPrefTest : IntegrationTestBase
    {
        private readonly CustomWebApplicationFactory _factory;

        public NotificationPrefTest(CustomWebApplicationFactory factory) : base(factory, factory.Services.GetRequiredService<ITokenService>())
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateNotificationPref_ValidData_ShouldCreateAndReturn201()
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
                IsOnClaimActive = true,
                IsOverdueActive = false,
                QuietHoursFrom = DateTimeOffset.UtcNow.AddHours(-2),
                QuietHoursTo = DateTimeOffset.UtcNow.AddHours(6)
            };

            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/notificationpref", body);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var responseDto = await response.Content.ReadFromJsonAsync<NotificationPref>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(responseDto);
            Assert.Equal(userId, responseDto.UserId);
            Assert.True(responseDto.IsEmailActive);
            Assert.False(responseDto.IsWhatsAppActive);
            Assert.True(responseDto.IsOnCheckInActive);
            Assert.True(responseDto.IsOnClaimActive);
            Assert.False(responseDto.IsOverdueActive);
        }

        [Fact]
        public async Task CreateNotificationPref_DuplicatePreferences_ShouldReturn409()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create existing notification preference
            var existingNp = new NotificationPref
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IsEmailActive = true,
                IsWhatsAppActive = true,
                IsOnCheckInActive = true,
                IsOnClaimActive = false,
                IsOverdueActive = true,
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            };

            await DbContext.NotificationPref.AddAsync(existingNp);
            await DbContext.SaveChangesAsync();

            var payload = new NotificationPrefCreateRequestDto
            {
                IsEmailActive = false,
                IsWhatsAppActive = false,
                IsOnCheckInActive = true,
                IsOnClaimActive = true,
                IsOverdueActive = true
            };

            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/notificationpref", body);

            Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task CreateNotificationPref_WithoutAuth_ShouldReturn401()
        {
            await ResetDatabaseAsync();

            var payload = new NotificationPrefCreateRequestDto
            {
                IsEmailActive = true,
                IsWhatsAppActive = true,
                IsOnCheckInActive = true,
                IsOnClaimActive = false,
                IsOverdueActive = true
            };

            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/notificationpref", body);

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetNotificationPrefById_ValidId_ShouldReturnNotificationPref()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var npId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var notificationPref = new NotificationPref
            {
                Id = npId,
                UserId = userId,
                IsEmailActive = true,
                IsWhatsAppActive = false,
                IsOnCheckInActive = true,
                IsOnClaimActive = false,
                IsOverdueActive = true,
                QuietHoursFrom = DateTimeOffset.UtcNow.AddHours(-3),
                QuietHoursTo = DateTimeOffset.UtcNow.AddHours(5),
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            };

            await DbContext.NotificationPref.AddAsync(notificationPref);
            await DbContext.SaveChangesAsync();

            var response = await Client.GetAsync($"api/v1/notificationpref/{npId}");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var responseDto = await response.Content.ReadFromJsonAsync<NotificationPrefResponseDto>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(responseDto);
            Assert.Equal(npId, responseDto.Id);
            Assert.Equal(userId, responseDto.UserId);
            Assert.True(responseDto.IsEmailActive);
            Assert.False(responseDto.IsWhatsAppActive);
            Assert.NotNull(responseDto.QuietHoursFrom);
            Assert.NotNull(responseDto.QuietHoursTo);
        }

        [Fact]
        public async Task GetNotificationPrefById_NotFound_ShouldReturn404()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var nonExistentId = Guid.NewGuid();
            var response = await Client.GetAsync($"api/v1/notificationpref/{nonExistentId}");

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetNotificationPrefById_WithoutAuth_ShouldReturn401()
        {
            await ResetDatabaseAsync();

            var npId = Guid.NewGuid();
            var response = await Client.GetAsync($"api/v1/notificationpref/{npId}");

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetNotificationPrefByUser_ValidUser_ShouldReturnNotificationPref()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var npId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var notificationPref = new NotificationPref
            {
                Id = npId,
                UserId = userId,
                IsEmailActive = false,
                IsWhatsAppActive = true,
                IsOnCheckInActive = false,
                IsOnClaimActive = true,
                IsOverdueActive = false,
                QuietHoursFrom = DateTimeOffset.UtcNow.AddHours(-1),
                QuietHoursTo = DateTimeOffset.UtcNow.AddHours(7),
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            };

            await DbContext.NotificationPref.AddAsync(notificationPref);
            await DbContext.SaveChangesAsync();

            var response = await Client.GetAsync("api/v1/notificationpref/me");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var responseDto = await response.Content.ReadFromJsonAsync<NotificationPrefResponseDto>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(responseDto);
            Assert.Equal(npId, responseDto.Id);
            Assert.Equal(userId, responseDto.UserId);
            Assert.False(responseDto.IsEmailActive);
            Assert.True(responseDto.IsWhatsAppActive);
            Assert.False(responseDto.IsOnCheckInActive);
            Assert.True(responseDto.IsOnClaimActive);
            Assert.False(responseDto.IsOverdueActive);
        }

        [Fact]
        public async Task GetNotificationPrefByUser_NoPreferences_ShouldReturn404()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await Client.GetAsync("api/v1/notificationpref/me");

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetNotificationPrefByUser_WithoutAuth_ShouldReturn401()
        {
            await ResetDatabaseAsync();

            var response = await Client.GetAsync("api/v1/notificationpref/me");

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateNotificationPref_WithQuietHours_ShouldCreateSuccessfully()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var quietStart = new DateTimeOffset(2025, 12, 24, 22, 0, 0, TimeSpan.Zero);
            var quietEnd = new DateTimeOffset(2025, 12, 25, 8, 0, 0, TimeSpan.Zero);

            var payload = new NotificationPrefCreateRequestDto
            {
                IsEmailActive = true,
                IsWhatsAppActive = true,
                IsOnCheckInActive = true,
                IsOnClaimActive = false,
                IsOverdueActive = true,
                QuietHoursFrom = quietStart,
                QuietHoursTo = quietEnd
            };

            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/notificationpref", body);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var responseDto = await response.Content.ReadFromJsonAsync<NotificationPref>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(responseDto);
            Assert.NotNull(responseDto.QuietHoursFrom);
            Assert.NotNull(responseDto.QuietHoursTo);
        }

        [Fact]
        public async Task CreateNotificationPref_WithAllNotificationsDisabled_ShouldCreateSuccessfully()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ResidentTest", "Resident");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var payload = new NotificationPrefCreateRequestDto
            {
                IsEmailActive = false,
                IsWhatsAppActive = false,
                IsOnCheckInActive = false,
                IsOnClaimActive = false,
                IsOverdueActive = false
            };

            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/notificationpref", body);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var responseDto = await response.Content.ReadFromJsonAsync<NotificationPref>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(responseDto);
            Assert.False(responseDto.IsEmailActive);
            Assert.False(responseDto.IsWhatsAppActive);
            Assert.False(responseDto.IsOnCheckInActive);
            Assert.False(responseDto.IsOnClaimActive);
            Assert.False(responseDto.IsOverdueActive);
        }

        [Fact]
        public async Task GetNotificationPrefById_AsWrongRole_ShouldBeAccessible()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var npId = Guid.NewGuid();
            
            // Create user with notification preferences
            await Seeder.GetLoginToken(userId, "ResidentUser", "Resident");
            var notificationPref = new NotificationPref
            {
                Id = npId,
                UserId = userId,
                IsEmailActive = true,
                IsWhatsAppActive = true,
                IsOnCheckInActive = true,
                IsOnClaimActive = false,
                IsOverdueActive = true,
                CreatedBy = userId,
                CreatedOn = DateTimeOffset.UtcNow
            };

            await DbContext.NotificationPref.AddAsync(notificationPref);
            await DbContext.SaveChangesAsync();

            // Try to access with Admin token
            var adminToken = await Seeder.GetLoginToken(adminId, "AdminUser", "Admin");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var response = await Client.GetAsync($"api/v1/notificationpref/{npId}");

            // Admin role should not be authorized for this endpoint (requires Resident role)
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}