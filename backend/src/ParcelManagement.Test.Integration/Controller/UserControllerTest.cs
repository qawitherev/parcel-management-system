using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ParcelManagement.Api.DTO;
using ParcelManagement.Api.DTO.V1;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Misc;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Test.Integration.Misc;

namespace ParcelManagement.Test.Integration
{
    public class UserControllerTest(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task RegisterResident_UsernameAlreadyExist_ShouldNotRegister()
        {
            var existingUsername = "the_username";
            // create the existing username
            // by using disposable dbContext 
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var existingUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = existingUsername,
                    Email = "the.email@email.com",
                    PasswordHash = "####",
                    PasswordSalt = "####"
                };
                await dbContext.ResidentUnits.AddAsync(new ResidentUnit
                {
                    Id = Guid.NewGuid(),
                    UnitName = "RU001"
                });
                await dbContext.Users.AddAsync(existingUser);
                await dbContext.SaveChangesAsync();
            }
            var registeringUser = new RegisterResidentDto
            {
                Username = existingUsername,
                Email = "email@email.com",
                Password = "Password123",
                ResidentUnit = "RU001"
            };

            var inJson = JsonConvert.SerializeObject(registeringUser);
            var content = new StringContent(inJson, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync("api/user/register/resident", content);
            Assert.Equal(System.Net.HttpStatusCode.Conflict, res.StatusCode);
        }

        [Fact]
        public async Task RegisterResident_UsernameNotExist_ShouldRegister()
        {
            var newUser = new RegisterResidentDto
            {
                Username = "user_1",
                Email = "this.email@email",
                Password = "Password_123",
                ResidentUnit = "RU001"
            };
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await dbContext.ResidentUnits.AddAsync(new ResidentUnit
                {
                    Id = Guid.NewGuid(),
                    UnitName = newUser.ResidentUnit
                });
                await dbContext.SaveChangesAsync();
            }
            var json = JsonConvert.SerializeObject(newUser);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync("api/user/register/resident", content);
            var deserialized = await res.Content.ReadFromJsonAsync<UserResponseDto>();
            Assert.Equal(System.Net.HttpStatusCode.Created, res.StatusCode);
            Assert.NotNull(res);
            Assert.Equal(newUser.Username, deserialized!.Username);
        }

        [Fact]
        public async Task UserLogin_UsernameNotFound_ShouldNotLogin()
        {
            var toBeLogin = new LoginDto
            {
                Username = "username_1",
                PlainPassword = "password_123"
            };
            var json = JsonConvert.SerializeObject(toBeLogin);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync("api/user/login", content);
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [Fact]
        public async Task UserLogin_InvalidCredential_ShouldNotLogin()
        {
            var theUsername = "username_01";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = theUsername,
                Email = "this@email.com",
                ResidentUnitDeprecated = "RU001",
                PasswordHash = "####",
                PasswordSalt = "####",
                Role = UserRole.Resident
            };
            user.PasswordHash = PasswordService.HashPassword(user, user.PasswordHash);
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();
                await dbContext.AddAsync(user);
                await dbContext.SaveChangesAsync();
            }
            var loginDto = new LoginDto
            {
                Username = theUsername,
                PlainPassword = "Password111"
            };
            var json = JsonConvert.SerializeObject(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync("api/user/login", content);
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [Fact]
        public async Task UserLogin_CorrectCredentials_ShouldLogin()
        {
            var theUsername = "username";
            var thePassword = "Password123";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = theUsername,
                Email = "email@email.com",
                ResidentUnitDeprecated = "RU001",
                PasswordHash = "####",
                PasswordSalt = "####"
            };
            user.PasswordHash = PasswordService.HashPassword(user, thePassword);
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await dbContext.Users.AddAsync(user);
                await dbContext.SaveChangesAsync();
            }
            var dto = new LoginDto
            {
                Username = theUsername,
                PlainPassword = thePassword
            };
            var content = IntegrationMisc.ConvertToStringContent(dto);
            var result = await _client.PostAsync("api/user/login", content);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Content);
        }
     }
}