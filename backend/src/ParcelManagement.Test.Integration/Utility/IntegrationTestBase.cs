using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParcelManagement.Api.AuthenticationAndAuthorization;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Test.Integration.Utility;

namespace ParcelManagement.Test.Integration
{
    public class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        protected readonly ApplicationDbContext DbContext;
        protected readonly IServiceScope Scope;
        protected readonly HttpClient Client;
        protected readonly TestDataSeeder Seeder;
        protected readonly ITokenService TokenService;

        public IntegrationTestBase(CustomWebApplicationFactory factory, ITokenService tokenService)
        {
            Scope = factory.Services.CreateScope();
            DbContext = Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Client = factory.CreateClient();
            TokenService = tokenService;
            Seeder = new TestDataSeeder(DbContext, tokenService);
        }

        public void Dispose()
        {
            Scope?.Dispose();
            Client?.Dispose();
            GC.SuppressFinalize(this);
        }

        public void ClearChangeTracker()
        {
            DbContext.ChangeTracker.Clear();
        }

        protected async Task  ResetDatabaseAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
        }
    }
}