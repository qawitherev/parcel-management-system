using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Services;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Infrastructure.Repository;
using Xunit;

namespace ParcelManagement.Test.Fixture
{
    public class ResidentUnitTestFixture : IAsyncLifetime
    {
        public ApplicationDbContext DbContext { get; private set; } = null!;
        public ResidentUnitRepository ResidentUnitRepository { get; private set; } = null!;
        public ResidentUnitService ResidentUnitService { get; private set; } = null!;

        public async Task DisposeAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.DisposeAsync();
        }

        public Task InitializeAsync()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            DbContext = new ApplicationDbContext(dbContextOptions);
            ResidentUnitRepository = new ResidentUnitRepository(DbContext);
            ResidentUnitService = new ResidentUnitService(ResidentUnitRepository);
            return Task.CompletedTask;
        }
    }
}