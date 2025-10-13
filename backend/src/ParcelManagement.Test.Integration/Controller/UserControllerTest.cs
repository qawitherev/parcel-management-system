using Microsoft.Extensions.DependencyInjection;
using ParcelManagement.Api.AuthenticationAndAuthorization;

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
            
        }
    }
}