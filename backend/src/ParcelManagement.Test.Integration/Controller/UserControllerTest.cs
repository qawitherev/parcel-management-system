using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ParcelManagement.Api.AuthenticationAndAuthorization;
using ParcelManagement.Api.DTO.V1;
using ParcelManagement.Core.Entities;
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
    }
}