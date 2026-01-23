// contains static helper method for the creation of test fixture 
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Test.Fixture
{
    public class TestFixtureHelper
    {
        public static DbContextOptions<ApplicationDbContext> GetDbContextOptions()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            return dbContextOptions;
        }

        public static async Task<ApplicationDbContext> GetSqlLiteDbContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection).Options;
            var dbContext = new ApplicationDbContext(dbContextOptions);
            await dbContext.Database.EnsureCreatedAsync();
            return dbContext;
        }
    }
}