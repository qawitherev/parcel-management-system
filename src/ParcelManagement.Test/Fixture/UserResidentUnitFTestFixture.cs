using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Services;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Infrastructure.Repository;
using Xunit;

namespace ParcelManagement.Test.Fixture
{
    public class UserResidentUnitTestFixture : IAsyncLifetime
    {
        public ApplicationDbContext DbContext { get; private set; } = null!;
        public UserResidentUnitRepository UserResidentUnitRepository { get; private set; } = null!;
        public UserResidentUnitService UserResidentUnitService { get; private set; } = null!;
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
            UserResidentUnitRepository = new UserResidentUnitRepository(DbContext);
            UserResidentUnitService = new UserResidentUnitService(UserResidentUnitRepository);
            return Task.CompletedTask;
        }

        public async Task ResetDb()
        {
            await DbContext.Database.EnsureDeletedAsync();
        }
    }
}