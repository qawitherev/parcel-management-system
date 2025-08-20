using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Services;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Infrastructure.Repository;
using Xunit;

namespace ParcelManagement.Test.Fixture
{

    // HOW TO USE 
    // Use this class when you want to have a shared database context across all tests 
    // in your test class 
    // HOW IT WORKS 
    // Conctructor will run once - setting up the fixtures you need for your test 
    // Dispose() will run once at the end of the test; hence the shared dbContext 
    public class UserTestFixture : IDisposable
    {
        public ApplicationDbContext DbContext { get; private set; }
        public UserRepository UserRepo { get; private set; }
        public UserService UserService { get; private set; }

        public UserTestFixture()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            DbContext = new ApplicationDbContext(dbContextOptions);

            UserRepo = new UserRepository(DbContext);
            UserService = new UserService(UserRepo);
        }
        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    // Second fixture that uses IAsyncLifetime for an isolated testing 
    // InitializeAsync will before each test class 
    // DisposeAsync is run at the end of the test class 
    // Still a shared dbContext but this one is async dispose 
    public class UserTestAsyncLifetimeFixture : IAsyncLifetime
    {
        public ApplicationDbContext DbContext { get; private set; } = null!;
        public UserRepository UserRepo { get; private set; } = null!;
        public UserService UserService { get; private set; } = null!;

        public Task InitializeAsync()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            DbContext = new ApplicationDbContext(dbContextOptions);
            UserRepo = new UserRepository(DbContext);
            UserService = new UserService(UserRepo);
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.DisposeAsync();
        }
    }
}