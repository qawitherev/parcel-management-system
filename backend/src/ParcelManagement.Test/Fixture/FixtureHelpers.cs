// contains static helper method for the creation of test fixture 
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
    }
}