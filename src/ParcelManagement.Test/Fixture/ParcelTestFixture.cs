using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Services;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Infrastructure.Repository;
using Xunit;

namespace ParcelManagement.Test.Fixture
{
    public class ParcelTestFixture : IAsyncLifetime
    {
        public ApplicationDbContext DbContext { get; private set; } = null!;
        public ParcelRepository ParcelRepo { get; private set; } = null!;
        public ParcelService ParcelService { get; private set; } = null!;

        public Task InitializeAsync()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            DbContext = new ApplicationDbContext(dbContextOptions);

            var residentUnitRepo = new ResidentUnitRepository(DbContext);
            var userRepo = new UserRepository(DbContext);
            var trackingEventRepo = new TrackingEventRepository(DbContext);
            ParcelRepo = new ParcelRepository(DbContext);
            ParcelService = new ParcelService(ParcelRepo, residentUnitRepo, userRepo, trackingEventRepo);
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.DisposeAsync();
        }

        public async Task ResetDb()
        {
            await DbContext.Database.EnsureDeletedAsync();
        }
    }
}