using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Test.Fixture;
using Xunit;

namespace ParcelManagement.Test.Repository
{
    public class UserResidentUnitRepositoryTest(
        UserResidentUnitTestFixture uruFixture
        ) : IClassFixture<UserResidentUnitTestFixture>
    {
        private readonly UserResidentUnitTestFixture _uruFixture = uruFixture;

        [Fact]
        public async Task CreateUserResidentUnitAsync_ShouldCreate()
        {
            var dbContext = _uruFixture.DbContext;

            var userId = Guid.NewGuid();
            var residentUnitId = Guid.NewGuid();
            var theId = Guid.NewGuid();

            var newUru = new UserResidentUnit
            {
                Id = theId,
                UserId = userId,
                ResidentUnitId = residentUnitId,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId,
            };

            await _uruFixture.UserResidentUnitRepository.CreateUserResidentUnitAsync(newUru);
            var res = await dbContext.UserResidentUnits.FindAsync(userId, residentUnitId);
            Assert.NotNull(res);
            Assert.Equal(residentUnitId, res.ResidentUnitId);
            Assert.Equal(userId, res.UserId);
            await _uruFixture.ResetDb();
        }

        [Fact]
        public async Task GetOneResidentUnitBySpecification_NotExist_ShouldReturnNull()
        {
            var spec = new UserResidentUnitByResidentUnitIdSpecification(Guid.NewGuid());
            var res = await _uruFixture.UserResidentUnitRepository.GetOneResidentUnitBySpecification(spec);
            Assert.Null(res);
            await _uruFixture.ResetDb();
        }

        [Fact]
        public async Task GetOneResidentUnitBySpecification_Exist_ShouldReturn()
        {
            var residentUnitId = Guid.NewGuid();
            var uru = new UserResidentUnit
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ResidentUnitId = residentUnitId,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid(),
            };

            await _uruFixture.DbContext.UserResidentUnits.AddAsync(uru);
            await _uruFixture.DbContext.SaveChangesAsync();

            var spec = new UserResidentUnitByResidentUnitIdSpecification(residentUnitId);
            var res = await _uruFixture.UserResidentUnitRepository
                .GetOneResidentUnitBySpecification(spec);

            Assert.NotNull(res);
            Assert.Equal(residentUnitId, res.ResidentUnitId);
            await _uruFixture.ResetDb();
        }

        [Fact]
        public async Task GetResidentUnitByUser_NotExist_ShouldReturnNull()
        {
            var res = await _uruFixture.UserResidentUnitRepository.GetResidentUnitsByUser(Guid.NewGuid());
            Assert.Empty(res);
        }

        [Fact]
        public async Task GetResidentUnitByUser_Exist_ShouldReturn()
        {
            var dbContext = _uruFixture.DbContext;

            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "",
                Email = "",
                PasswordHash = "",
                PasswordSalt = ""
            };

            var residentUnit1Id = Guid.NewGuid();
            var residentUnit2Id = Guid.NewGuid();
            var residentUnits = new ResidentUnit[] {
                new() {
                    Id = residentUnit1Id,
                    UnitName = "Unit 1"
                },
                new() {
                    Id = residentUnit2Id,
                    UnitName = "Unit 2"
                }
            };
            var urus = new UserResidentUnit[] {
                new() {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ResidentUnitId = residentUnit1Id,
                    IsActive = true
                },
                new() {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ResidentUnitId = residentUnit2Id,
                    IsActive = true
                }
             };

            await dbContext.Users.AddAsync(user);
            await dbContext.ResidentUnits.AddRangeAsync(residentUnits);
            await dbContext.UserResidentUnits.AddRangeAsync(urus);
            await dbContext.SaveChangesAsync();

            var res = await _uruFixture.UserResidentUnitRepository.GetResidentUnitsByUser(userId);
            Assert.NotNull(res);
            Assert.Equal(residentUnits.Length, res.Count);
            foreach (var unit in res)
            {
                Assert.NotNull(unit);
            }
            foreach (var unit in residentUnits)
            {
                Assert.Contains(res, res => res!.UnitName == unit.UnitName);
            }
            await _uruFixture.ResetDb();
        }

        [Fact]
        public async Task GetUserByResidentUnit_NotExist_ShouldReturnNull()
        {
            var res = await _uruFixture.UserResidentUnitRepository.GetUsersByResidentUnit(Guid.NewGuid());
            Assert.Empty(res);
        }

        [Fact]
        public async Task GetUserByResidentUnit_Exist_ShouldReturnUsers()
        {
            var dbContext = _uruFixture.DbContext;
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            var users = new User[] {
                new() {
                Id = user1Id,
                Username = "",
                Email = "",
                PasswordHash = "",
                PasswordSalt = ""
                },
                new() {
                Id = user2Id,
                Username = "",
                Email = "",
                PasswordHash = "",
                PasswordSalt = "" }
            };
            var residentUnitId = Guid.NewGuid();
            var residentUnit = new ResidentUnit
            {
                Id = residentUnitId,
                UnitName = "Unit-1"
            };
            var urus = new UserResidentUnit[] {
                new() {
                    Id = Guid.NewGuid(),
                    UserId = user1Id,
                    ResidentUnitId = residentUnitId,
                    IsActive = true
                },
                new() {
                    Id = Guid.NewGuid(),
                    UserId = user2Id,
                    ResidentUnitId = residentUnitId

                }
            };
            await dbContext.Users.AddRangeAsync(users);
            await dbContext.ResidentUnits.AddAsync(residentUnit);
            await dbContext.UserResidentUnits.AddRangeAsync(urus);
            await dbContext.SaveChangesAsync();

            var res = await _uruFixture.UserResidentUnitRepository.GetUsersByResidentUnit(residentUnitId);
            foreach (var r in res)
            {
                Assert.NotNull(r);
            }
            foreach (var user in users)
            {
                Assert.Contains(res, res => res!.Id == user.Id);
            }

            await _uruFixture.ResetDb();
        }
    }
}