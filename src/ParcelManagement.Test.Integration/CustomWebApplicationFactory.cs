using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        }
    }
}