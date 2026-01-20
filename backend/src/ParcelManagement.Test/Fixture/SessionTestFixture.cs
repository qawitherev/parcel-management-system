using ParcelManagement.Core.Services;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Infrastructure.Repository;
using Xunit;

namespace ParcelManagement.Test.Fixture
{
    public class SessionTestFixture : IAsyncLifetime
    {
        public ApplicationDbContext DbContext { get; private set; } = null!;
        public SessionService SessionService { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            DbContext = await TestFixtureHelper.GetSqlLiteDbContext();
            SessionService = new SessionService(new SessionRepository(DbContext));
        }

        public async Task DisposeAsync()
        {
            await DbContext.DisposeAsync();
        }
    }
}