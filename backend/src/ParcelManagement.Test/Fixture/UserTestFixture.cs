using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ParcelManagement.Infrastructure.Database;
using Xunit;

namespace ParcelManagement.Test.Fixture
{
    public class UserTestFixture : IAsyncLifetime
    {
        public ApplicationDbContext DbContext { get; private set; } = null!;
        private SqliteConnection  _connection = null!;

        public async Task InitializeAsync()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            await _connection.OpenAsync();
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection).Options;
            DbContext = new ApplicationDbContext(dbContextOptions);

            await DbContext.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.DisposeAsync();
        }
    }
}