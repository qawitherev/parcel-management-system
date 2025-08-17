using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ParcelManagement.Api.DTO;
using ParcelManagement.Core.Entities;
using ParcelManagement.Infrastructure.Database;

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
                await dbContext.Users.AddAsync(existingUser);
                await dbContext.SaveChangesAsync();
            }
            var registeringUser = new RegisterResidentDto
            {
                Username = existingUsername,
                Email = "email@email.com",
                PlainPassword = "Password123",
                ResidentUnit = "RU001"
            };

            var inJson = JsonConvert.SerializeObject(registeringUser);
            var content = new StringContent(inJson, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync("api/user/register/resident", content);
            Assert.Equal(System.Net.HttpStatusCode.Conflict, res.StatusCode);
        }
     }
}