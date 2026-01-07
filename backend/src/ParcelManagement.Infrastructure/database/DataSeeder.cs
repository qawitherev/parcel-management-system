using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Misc;
using ParcelManagement.Infrastructure.Misc;

namespace ParcelManagement.Infrastructure.Database
{
    public class AdminDataSeeder
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly SystemAdmin _systemAdmin;
        public AdminDataSeeder(
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            IOptions<SystemAdmin> systemAdmin
        )
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _systemAdmin = systemAdmin.Value;
        }

        public async Task SeedAsync()
        {
            var username = "system-admin";
            var email = _configuration["Admin:Email"] ?? _systemAdmin.Email ?? "admin@parcelSystem.com";
            var password = _configuration["Admin:Password"] ?? _systemAdmin.Password ??
                throw new InvalidOperationException("Admin password not found");
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
                adminUser.PasswordHash = PasswordService.HashPlainPasswordOrToken(adminUser, password);
                await _dbContext.Users.AddAsync(adminUser);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}