using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using ParcelManagement.Core.Services;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Infrastructure.Repository;
using Xunit;

namespace ParcelManagement.Test.Fixture
{
    public class TrackingEventTestFixture : IAsyncLifetime
    {
        public ApplicationDbContext DbContext { get; private set; } = null!;
        public TrackingEventRepository TrackingEventRepo { get; private set; } = null!;
        public TrackingEventService TrackingEventService { get; private set; } = null!;
        public async Task DisposeAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.DisposeAsync();
        }

        public Task InitializeAsync()
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            DbContext = new ApplicationDbContext(dbOptions);
            TrackingEventRepo = new TrackingEventRepository(DbContext);
            var parcelRepo = new ParcelRepository(DbContext);
            TrackingEventService = new TrackingEventService(TrackingEventRepo, parcelRepo);
            return Task.CompletedTask;
        }

        public async Task ResetDb()
        {
            await DbContext.Database.EnsureDeletedAsync();
        }
    }
}