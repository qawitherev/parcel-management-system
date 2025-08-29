using ParcelManagement.Core.Entities;
using ParcelManagement.Test.Fixture;
using Xunit;

namespace ParcelManagement.Test.Service
{
    public class ResidentUnitServiceTest(
        ResidentUnitTestFixture fixture
    ) : IClassFixture<ResidentUnitTestFixture>
    {
        private readonly ResidentUnitTestFixture _fixture = fixture;

        [Fact]
        public async Task CreateResidentUnitAsync_UnitExisted_ShouldNotCreateResidentUnit()
        {
            var dbContext = _fixture.DbContext;
            var theUnitName = "RU001";
            var existing = new ResidentUnit
            {
                Id = Guid.NewGuid(),
                UnitName = theUnitName,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            };

            var newResidentUnit = new ResidentUnit
            {
                Id = Guid.NewGuid(),
                UnitName = theUnitName,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            };
            await dbContext.ResidentUnits.AddAsync(existing);
            await dbContext.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _fixture.ResidentUnitService.CreateResidentUnitAsync(newResidentUnit.UnitName, Guid.NewGuid());
            });

            await _fixture.ResetDb();
        }

        [Fact]
        public async Task CreateResidentUnitAsync_UnitDoNotExisted_ShouldCreateUnit()
        {
            var theId = Guid.NewGuid();
            var newResidentUnit = new ResidentUnit
            {
                Id = theId,
                UnitName = "RU001",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            };
            await _fixture.ResidentUnitService.CreateResidentUnitAsync(newResidentUnit.UnitName, Guid.NewGuid());
            var res = await _fixture.DbContext.ResidentUnits.FindAsync(theId);
            Assert.NotNull(res);
            Assert.Equal(theId, res.Id);

            await _fixture.ResetDb();
        }

        [Fact]
        public async Task UpdateResidentUnit_UnitDoesNotExist_ShouldThrowError()
        {
            var residentUnit = new ResidentUnit
            {
                Id = Guid.NewGuid(),
                UnitName = "RU001",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            };
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await _fixture.ResidentUnitService.UpdateResidentUnitAsync(residentUnit);
            });

            await _fixture.ResetDb();
        }

        [Fact]
        public async Task UpdateResidentUnitAsync_ValidUnit_ShouldUpdateUnit()
        {
            var dbContext = _fixture.DbContext;
            var oldUnitName = "RU001";
            var newUnitName = "RU001-updated";
            var theId = Guid.NewGuid();
            var residentUnit = new ResidentUnit
            {
                Id = theId,
                UnitName = oldUnitName,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            };

            await dbContext.ResidentUnits.AddAsync(residentUnit);
            await dbContext.SaveChangesAsync();

            residentUnit.UnitName = newUnitName;
            await _fixture.ResidentUnitService.UpdateResidentUnitAsync(residentUnit);
            var res = await dbContext.ResidentUnits.FindAsync(theId);
            Assert.NotNull(res);
            Assert.Equal(newUnitName, res.UnitName);
        }
    }
}