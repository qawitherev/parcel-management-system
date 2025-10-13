using ParcelManagement.Core.Entities;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Test.Integration.Utility
{
    public class TestDataSeeder
    {
        private readonly ApplicationDbContext _dbContext;
        public TestDataSeeder(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Parcel> SeedParcelAsync()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "TestParcelManager",
                Email = "testParcelManager@test.com",
                PasswordHash = "########",
                Role = UserRole.ParcelRoomManager,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var locker = new Locker
            {
                Id = Guid.NewGuid(),
                LockerName = "Locker001",
                CreatedBy = user.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };

            var residentUnit = new ResidentUnit
            {
                Id = Guid.NewGuid(),
                UnitName = "RU001",
                CreatedBy = user.Id
            };

            var parcel = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = "TN001",
                LockerId = locker.Id,
                ResidentUnitId = residentUnit.Id,
                EntryDate = DateTimeOffset.UtcNow,
                Status = ParcelStatus.AwaitingPickup,
                Version = 2
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Lockers.AddAsync(locker);
            await _dbContext.ResidentUnits.AddAsync(residentUnit);
            await _dbContext.Parcels.AddAsync(parcel);
            await _dbContext.SaveChangesAsync();

            return parcel;
        }
    }
}