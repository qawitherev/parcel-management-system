using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ParcelManagement.Api.AuthenticationAndAuthorization;
using ParcelManagement.Api.DTO.V1;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Misc;
using ParcelManagement.Test.Integration.Misc;

namespace ParcelManagement.Test.Integration
{
    public class UserControllerTest : IntegrationTestBase
    {
        private readonly CustomWebApplicationFactory _factory;
        public UserControllerTest(CustomWebApplicationFactory factory) : base(factory, factory.Services.GetRequiredService<ITokenService>())
        {
            _factory = factory;
        }

        [Fact]
        public async Task RegisterResident_ValidData_ShouldRegister()
        {
            await ResetDatabaseAsync();
            await Seeder.SeedResidentUnitAsync();

            var payload = new RegisterResidentDto
            {
                Username = "TestRegisterUser",
                Email = "testEmail@test.com",
                Password = "Password123",
                ResidentUnit = "RU001"
            };
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync($"api/v1/user/register/resident", body);
            var responseInPoco = await response.Content.ReadFromJsonAsync<UserResponseDto>(IntegrationMisc.GetJsonSerializerOptions());

            Assert.NotNull(responseInPoco);
            Assert.Equal(payload.Username, responseInPoco.Username);
        }

        [Fact]
        public async Task RegisterResident_InvalidUsername_ShouldReturn()
        {
            await ResetDatabaseAsync();
            // we use get login token seeder because it is inserting user 
            var theUsername = "existingUser";
            await Seeder.GetLoginToken(Guid.NewGuid(), theUsername, "Resident");
            await Seeder.SeedResidentUnitAsync();

            var payload = new RegisterResidentDto
            {
                Username = theUsername,
                Email = "testEmail@test.com",
                Password = "Password123",
                ResidentUnit = "RU001"
            };
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync($"api/v1/user/register/resident", body);
            Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task UserLogin_ValidCredentials_ShouldReturnTokens()
        {
            await ResetDatabaseAsync();

            // Arrange - Create a user with hashed password
            var userId = Guid.NewGuid();
            var username = "LoginTestUser";
            var plainPassword = "TestPassword123";
            var email = "logintest@test.com";
            var role = UserRole.Resident;

            var user = new User
            {
                Id = userId,
                Username = username,
                Email = email,
                PasswordHash = "tempHash", // Will be replaced
                Role = role,
                CreatedAt = DateTimeOffset.UtcNow
            };

            // Hash the password properly
            user.PasswordHash = PasswordService.HashPlainPasswordOrToken(user, plainPassword);
            
            await DbContext.Users.AddAsync(user);
            await DbContext.SaveChangesAsync();

            var payload = new LoginDto
            {
                Username = username,
                PlainPassword = plainPassword
            };

            // Act
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/user/login", body);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(loginResponse);
            Assert.NotNull(loginResponse.AccessToken);
            Assert.NotEmpty(loginResponse.AccessToken);
            Assert.NotNull(loginResponse.RefreshToken);
            Assert.NotEmpty(loginResponse.RefreshToken);
        }

        [Fact]
        public async Task UserLogin_InvalidUsername_ShouldReturnUnauthorized()
        {
            await ResetDatabaseAsync();

            // Arrange
            var payload = new LoginDto
            {
                Username = "NonExistentUser",
                PlainPassword = "SomePassword123"
            };

            // Act
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/user/login", body);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UserLogin_InvalidPassword_ShouldReturnUnauthorized()
        {
            await ResetDatabaseAsync();

            // Arrange - Create a user
            var userId = Guid.NewGuid();
            var username = "LoginTestUser2";
            var plainPassword = "CorrectPassword123";
            
            var user = new User
            {
                Id = userId,
                Username = username,
                Email = "test2@test.com",
                PasswordHash = "tempHash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            user.PasswordHash = PasswordService.HashPlainPasswordOrToken(user, plainPassword);
            
            await DbContext.Users.AddAsync(user);
            await DbContext.SaveChangesAsync();

            var payload = new LoginDto
            {
                Username = username,
                PlainPassword = "WrongPassword123"
            };

            // Act
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/user/login", body);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UserLogin_AdminRole_ShouldReturnTokens()
        {
            await ResetDatabaseAsync();

            // Arrange - Create an admin user
            var userId = Guid.NewGuid();
            var username = "AdminUser";
            var plainPassword = "AdminPassword123";
            
            var user = new User
            {
                Id = userId,
                Username = username,
                Email = "admin@test.com",
                PasswordHash = "tempHash",
                Role = UserRole.Admin,
                CreatedAt = DateTimeOffset.UtcNow
            };
            user.PasswordHash = PasswordService.HashPlainPasswordOrToken(user, plainPassword);
            
            await DbContext.Users.AddAsync(user);
            await DbContext.SaveChangesAsync();

            var payload = new LoginDto
            {
                Username = username,
                PlainPassword = plainPassword
            };

            // Act
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/user/login", body);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(loginResponse);
            Assert.NotNull(loginResponse.AccessToken);
            Assert.NotEmpty(loginResponse.AccessToken);
        }

        [Fact]
        public async Task UserLogin_ParcelRoomManagerRole_ShouldReturnTokens()
        {
            await ResetDatabaseAsync();

            // Arrange - Create a parcel room manager user
            var userId = Guid.NewGuid();
            var username = "ManagerUser";
            var plainPassword = "ManagerPassword123";
            
            var user = new User
            {
                Id = userId,
                Username = username,
                Email = "manager@test.com",
                PasswordHash = "tempHash",
                Role = UserRole.ParcelRoomManager,
                CreatedAt = DateTimeOffset.UtcNow
            };
            user.PasswordHash = PasswordService.HashPlainPasswordOrToken(user, plainPassword);
            
            await DbContext.Users.AddAsync(user);
            await DbContext.SaveChangesAsync();

            var payload = new LoginDto
            {
                Username = username,
                PlainPassword = plainPassword
            };

            // Act
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/user/login", body);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(loginResponse);
            Assert.NotNull(loginResponse.AccessToken);
            Assert.NotEmpty(loginResponse.AccessToken);
        }

        [Fact]
        public async Task UserLogin_EmptyUsername_ShouldReturnBadRequest()
        {
            await ResetDatabaseAsync();

            // Arrange
            var payload = new LoginDto
            {
                Username = "",
                PlainPassword = "SomePassword123"
            };

            // Act
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/user/login", body);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UserLogin_EmptyPassword_ShouldReturnBadRequest()
        {
            await ResetDatabaseAsync();

            // Arrange
            var payload = new LoginDto
            {
                Username = "TestUser",
                PlainPassword = ""
            };

            // Act
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/user/login", body);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UserLogin_MultipleLoginAttempts_ShouldSucceedEachTime()
        {
            await ResetDatabaseAsync();

            // Arrange - Create a user
            var userId = Guid.NewGuid();
            var username = "MultiLoginUser";
            var plainPassword = "TestPassword123";
            
            var user = new User
            {
                Id = userId,
                Username = username,
                Email = "multilogin@test.com",
                PasswordHash = "tempHash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            user.PasswordHash = PasswordService.HashPlainPasswordOrToken(user, plainPassword);
            
            await DbContext.Users.AddAsync(user);
            await DbContext.SaveChangesAsync();

            var payload = new LoginDto
            {
                Username = username,
                PlainPassword = plainPassword
            };

            // Act - First login
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response1 = await Client.PostAsync("api/v1/user/login", body);
            Assert.Equal(System.Net.HttpStatusCode.OK, response1.StatusCode);

            // Act - Second login
            var response2 = await Client.PostAsync("api/v1/user/login", body);
            Assert.Equal(System.Net.HttpStatusCode.OK, response2.StatusCode);

            // Act - Third login
            var response3 = await Client.PostAsync("api/v1/user/login", body);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response3.StatusCode);
            var loginResponse = await response3.Content.ReadFromJsonAsync<LoginResponseDto>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(loginResponse);
            Assert.NotNull(loginResponse.AccessToken);
            Assert.NotEmpty(loginResponse.AccessToken);
        }

        [Fact]
        public async Task UserLogin_SpecialCharactersInPassword_ShouldWork()
        {
            await ResetDatabaseAsync();

            // Arrange - Create a user with special characters in password
            var userId = Guid.NewGuid();
            var username = "SpecialCharUser";
            var plainPassword = "P@ssw0rd!#$%";
            
            var user = new User
            {
                Id = userId,
                Username = username,
                Email = "special@test.com",
                PasswordHash = "tempHash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            user.PasswordHash = PasswordService.HashPlainPasswordOrToken(user, plainPassword);
            
            await DbContext.Users.AddAsync(user);
            await DbContext.SaveChangesAsync();

            var payload = new LoginDto
            {
                Username = username,
                PlainPassword = plainPassword
            };

            // Act
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/user/login", body);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(loginResponse);
            Assert.NotNull(loginResponse.AccessToken);
        }

        [Fact]
        public async Task UserLogin_DifferentUsersSimultaneously_ShouldReturnDifferentTokens()
        {
            await ResetDatabaseAsync();

            // Arrange - Create two different users
            var user1Id = Guid.NewGuid();
            var user1Username = "User1";
            var user1Password = "Password1";
            
            var user1 = new User
            {
                Id = user1Id,
                Username = user1Username,
                Email = "user1@test.com",
                PasswordHash = "tempHash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            user1.PasswordHash = PasswordService.HashPlainPasswordOrToken(user1, user1Password);

            var user2Id = Guid.NewGuid();
            var user2Username = "User2";
            var user2Password = "Password2";
            
            var user2 = new User
            {
                Id = user2Id,
                Username = user2Username,
                Email = "user2@test.com",
                PasswordHash = "tempHash",
                Role = UserRole.Admin,
                CreatedAt = DateTimeOffset.UtcNow
            };
            user2.PasswordHash = PasswordService.HashPlainPasswordOrToken(user2, user2Password);
            
            await DbContext.Users.AddRangeAsync(user1, user2);
            await DbContext.SaveChangesAsync();

            // Act - Login both users
            var payload1 = new LoginDto
            {
                Username = user1Username,
                PlainPassword = user1Password
            };
            var body1 = IntegrationMisc.ConvertToStringContent(payload1);
            var response1 = await Client.PostAsync("api/v1/user/login", body1);

            var payload2 = new LoginDto
            {
                Username = user2Username,
                PlainPassword = user2Password
            };
            var body2 = IntegrationMisc.ConvertToStringContent(payload2);
            var response2 = await Client.PostAsync("api/v1/user/login", body2);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response1.StatusCode);
            Assert.Equal(System.Net.HttpStatusCode.OK, response2.StatusCode);

            var loginResponse1 = await response1.Content.ReadFromJsonAsync<LoginResponseDto>(IntegrationMisc.GetJsonSerializerOptions());
            var loginResponse2 = await response2.Content.ReadFromJsonAsync<LoginResponseDto>(IntegrationMisc.GetJsonSerializerOptions());

            Assert.NotNull(loginResponse1);
            Assert.NotNull(loginResponse2);
            Assert.NotEqual(loginResponse1.AccessToken, loginResponse2.AccessToken);
            Assert.NotEqual(loginResponse1.RefreshToken, loginResponse2.RefreshToken);
        }

        [Fact]
        public async Task UserLogin_LongPassword_ShouldWork()
        {
            await ResetDatabaseAsync();

            // Arrange - Create a user with a long password
            var userId = Guid.NewGuid();
            var username = "LongPasswordUser";
            var plainPassword = "ThisIsAVeryLongPasswordWithMoreThan50CharactersToTestTheSystem123!@#";
            
            var user = new User
            {
                Id = userId,
                Username = username,
                Email = "longpass@test.com",
                PasswordHash = "tempHash",
                Role = UserRole.Resident,
                CreatedAt = DateTimeOffset.UtcNow
            };
            user.PasswordHash = PasswordService.HashPlainPasswordOrToken(user, plainPassword);
            
            await DbContext.Users.AddAsync(user);
            await DbContext.SaveChangesAsync();

            var payload = new LoginDto
            {
                Username = username,
                PlainPassword = plainPassword
            };

            // Act
            var body = IntegrationMisc.ConvertToStringContent(payload);
            var response = await Client.PostAsync("api/v1/user/login", body);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(loginResponse);
            Assert.NotNull(loginResponse.AccessToken);
        }
    }
}