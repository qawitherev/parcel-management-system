using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Test.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // just leave this base implementation here because its not doing anything 
            // but a reminder to us to jangan jadi kacang yang lupakan kulit 
            base.ConfigureWebHost(builder);

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