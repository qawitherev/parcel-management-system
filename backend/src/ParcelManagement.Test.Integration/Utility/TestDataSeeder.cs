using ParcelManagement.Api.AuthenticationAndAuthorization;
using ParcelManagement.Api.Utility;
using ParcelManagement.Core.Entities;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Test.Integration.Utility
{
    public class TestDataSeeder
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ITokenService _tokenService;
        public TestDataSeeder(ApplicationDbContext dbContext, ITokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        public async Task<string> GetLoginToken(Guid userId, string username, string role)
        {
            var theRole = EnumUtils.ToEnumOrNull<UserRole>(role) ?? throw new InvalidCastException("Role is invalid");
            var user = new User
            {
                Id = userId,
                Username = username,
                Email = "testParcelManager@test.com",
                PasswordHash = "########",
                Role = theRole,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var token = _tokenService.GenerateAccessToken(user.Id.ToString(), user.Username, role);
            return token;
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

        public async Task SeedForCheckIn(Guid checkingInUser)
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
            await _dbContext.Users.AddAsync(user);
            await _dbContext.Lockers.AddAsync(locker);
            await _dbContext.ResidentUnits.AddAsync(residentUnit);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<ResidentUnit> SeedResidentUnitAsync()
        {
            var user = GetUser();
            var residentUnit = new ResidentUnit
            {
                Id = Guid.NewGuid(),
                UnitName = "RU001",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = user.Id
            };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.ResidentUnits.AddAsync(residentUnit);
            await _dbContext.SaveChangesAsync();
            return residentUnit;
        }
        
        private static User GetUser()
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
            return user;
        }

        public async Task SeedForBulkClaim(Guid userId)
        {
            var residentUnit = new ResidentUnit
            {
                Id = Guid.NewGuid(),
                UnitName = "RU001",
                CreatedBy = userId
            };

            var userResidentUnit = new UserResidentUnit
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ResidentUnitId = residentUnit.Id,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            var parcel1 = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = "TN001",
                ResidentUnitId = residentUnit.Id,
                EntryDate = DateTimeOffset.UtcNow,
                Status = ParcelStatus.AwaitingPickup,
                Version = 1
            };

            var parcel2 = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = "TN002",
                ResidentUnitId = residentUnit.Id,
                EntryDate = DateTimeOffset.UtcNow,
                Status = ParcelStatus.AwaitingPickup,
                Version = 1
            };

            await _dbContext.ResidentUnits.AddAsync(residentUnit);
            await _dbContext.UserResidentUnits.AddAsync(userResidentUnit);
            await _dbContext.Parcels.AddRangeAsync(parcel1, parcel2);
            await _dbContext.SaveChangesAsync();
        }

        public async Task SeedForBulkClaimWithClaimedParcel(Guid userId)
        {
            var residentUnit = new ResidentUnit
            {
                Id = Guid.NewGuid(),
                UnitName = "RU001",
                CreatedBy = userId
            };

            var userResidentUnit = new UserResidentUnit
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ResidentUnitId = residentUnit.Id,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            var parcel1 = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = "TN001",
                ResidentUnitId = residentUnit.Id,
                EntryDate = DateTimeOffset.UtcNow,
                Status = ParcelStatus.AwaitingPickup,
                Version = 1
            };

            var parcel2 = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = "TN002",
                ResidentUnitId = residentUnit.Id,
                EntryDate = DateTimeOffset.UtcNow,
                Status = ParcelStatus.Claimed,
                ExitDate = DateTimeOffset.UtcNow,
                Version = 1
            };

            await _dbContext.ResidentUnits.AddAsync(residentUnit);
            await _dbContext.UserResidentUnits.AddAsync(userResidentUnit);
            await _dbContext.Parcels.AddRangeAsync(parcel1, parcel2);
            await _dbContext.SaveChangesAsync();
        }
    }
}