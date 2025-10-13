using Microsoft.Extensions.DependencyInjection;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Test.Integration.Utility;

namespace ParcelManagement.Test.Integration
{
    public class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
    {
        protected readonly ApplicationDbContext DbContext;
        protected readonly IServiceScope Scope;
        protected readonly HttpClient Client;
        protected readonly TestDataSeeder Seeder;

        public IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            Scope = factory.Services.CreateScope();
            DbContext = Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Client = factory.CreateClient();
            Seeder = new TestDataSeeder(DbContext);
        }

        protected async Task  ResetDatabaseAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
        }
    }
}