using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ParcelManagement.Infrastructure.Database
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "../ParcelManagement.Api");
            var apiAssemblyPath = Path.Combine(apiProjectPath, "bin/debug/net9.0/ParcelManagement.Api.dll");
            var apiAssembly = Assembly.LoadFrom(apiAssemblyPath);

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../ParcelManagement.Api"))
                .AddJsonFile("appsettings.json")
                .AddUserSecrets(apiAssembly, optional:true);


            var config = configBuilder.Build();

            //check if connectionString doesn't exist 
            var connectionString = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is DefaultConnection is empty");
            }

            var options = new DbContextOptionsBuilder<ApplicationDbContext>();
            options.UseMySql(config.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(config.GetConnectionString("DefaultConnection")));


            return new ApplicationDbContext(options.Options);
        }
    }
}