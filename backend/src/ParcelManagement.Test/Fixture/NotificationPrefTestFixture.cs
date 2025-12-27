using ParcelManagement.Core.Services;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Infrastructure.Repository;
using Xunit;

namespace ParcelManagement.Test.Fixture 
{
    public class NotificationPrefFixture : IAsyncLifetime
    {
        public ApplicationDbContext DbContext { get; private set; } = null!;
        public NotificationPrefRepository NpRepo { get; private set; } = null!; 
        public NotificationPrefService NpService { get; private set; } = null!;


        public async Task DisposeAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.DisposeAsync();
        }

        public Task InitializeAsync()
        {
            var dbContextOptions = TestFixtureHelper.GetDbContextOptions();
            DbContext = new ApplicationDbContext(dbContextOptions);

            NpRepo = new NotificationPrefRepository(DbContext);
            var userRepo = new UserRepository(DbContext);
            NpService = new NotificationPrefService(NpRepo, userRepo);

            return Task.CompletedTask;
        }
    }
}