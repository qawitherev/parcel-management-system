using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParcelManagement.Infrastructure.Database;
using Testcontainers.MySql;
using Testcontainers.Redis;

namespace ParcelManagement.Test.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<ParcelManagement.Api.Program>, IAsyncLifetime
    {

        private readonly MySqlContainer _mySqlContainer;
        private readonly RedisContainer _redisContainer; 

        public CustomWebApplicationFactory()
        {
            _mySqlContainer = new MySqlBuilder()
                .WithImage("mysql:8.0")
                .WithDatabase("integrationTestDB")
                .WithUsername("TestAdmin")
                .WithPassword("AdminPassword123")
                .WithCleanUp(true)
                .Build();

            _redisContainer = new RedisBuilder("redis:7.0")
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _mySqlContainer.StartAsync(); // --> will spin up a docker container 
            await _redisContainer.StartAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var existingDbContext = services.SingleOrDefault(c => c.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (existingDbContext != null)
                {
                    services.Remove(existingDbContext);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseMySql(
                        _mySqlContainer.GetConnectionString(),
                        ServerVersion.AutoDetect(_mySqlContainer.GetConnectionString())
                    );
                });
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                dbContext.Database.EnsureCreated();
            });

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear();
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JWTSettings:SecretKey"] = "this-is-a-very-very-very-very-long-secret-key-for-testing",
                    ["JWTSettings:Issuer"] = "test-issuer",
                    ["JWTSettings:Audience"] = "test-audience",
                    ["JWTSettings:ExpirationMinutes"] = "60",
                    ["Admin:Email"] = "admin@parcelSystem.com",
                    ["Admin:Password"] = "this-is-admin-password", 
                    ["RedisSettings:ConnectionString"] = _redisContainer.GetConnectionString()
                });
            });
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _mySqlContainer.DisposeAsync();
            await _redisContainer.DisposeAsync();
        }
    }
}