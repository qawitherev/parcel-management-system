using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ParcelManagement.Infrastructure.Database
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var projectRoot = Path.GetFullPath(
                                Path.Combine(Directory.GetCurrentDirectory(),
                                             "..", "ParcelManagement.Api"));
            DotNetEnv.Env.Load(Path.Combine(projectRoot, ".env"));
            var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            
            if (string.IsNullOrWhiteSpace(cs))
            {
                throw new InvalidOperationException("Connection string is empty");
            }

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseMySql(cs, ServerVersion.AutoDetect(cs));

            return new ApplicationDbContext(builder.Options);
        }
    }
}