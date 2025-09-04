using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Misc;

namespace ParcelManagement.Infrastructure.Database
{
    public class AdminDataSeeder
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        public AdminDataSeeder(
            ApplicationDbContext dbContext,
            IConfiguration configuration
        )
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task SeedAsync()
        {
            var username = "system-admin";
            var email = _configuration["Admin:Email"] ?? "admin@parcelSystem.com";
            var password = _configuration["Admin:Password"] ?? throw new InvalidOperationException("Admin password not found");
            var hasAdmin = await _dbContext.Users.Where(u => u.Username == username).FirstOrDefaultAsync();
            if (hasAdmin == null)
            {
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    Email = email,
                    PasswordHash = "####",
                    Role = UserRole.Admin
                };
                adminUser.PasswordHash = PasswordService.HashPassword(adminUser, password);
                await _dbContext.Users.AddAsync(adminUser);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}