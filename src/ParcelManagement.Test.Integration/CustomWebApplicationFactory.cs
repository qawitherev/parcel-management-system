using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ParcelManagement.Api.AuthenticationAndAuthorization;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Test.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<ParcelManagement.Api.Program>
    {
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
                services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("integrationTestDb"));
            });

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<JWTSettings>();
                var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JWTSettings:SecretKey"] = "this-is-a-very-very-very-very-long-secret-key-for-testing",
                    ["JWTSettings:Issuer"] = "test-issuer",
                    ["JWTSettings:Audience"] = "test-audience",
                    ["JWTSettings:ExpirationMinutes"] = "60"
                }).Build();
                services.Configure<JWTSettings>(configuration.GetSection("JWTSettings"));
            });
        }
    }
}