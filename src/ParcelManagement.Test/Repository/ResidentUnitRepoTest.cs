using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Test.Fixture;
using Xunit;

namespace ParcelManagement.Test.Repository
{
    public class ResidentUnitRepoTest(ResidentUnitTestFixture fixture) : IClassFixture<ResidentUnitTestFixture>
    {
        private readonly ResidentUnitTestFixture _fixture = fixture;

        [Fact]
        public async Task CreateResidentUnitAsync_ShouldCreateUser()
        {
            var residentUnitRepo = _fixture.ResidentUnitRepository;
            var theId = Guid.NewGuid();
            var newResidentUnit = new ResidentUnit
            {
                Id = theId,
                UnitName = "RU001",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            };
            await residentUnitRepo.CreateResidentUnitAsync(newResidentUnit);
            var res = await _fixture.DbContext.ResidentUnits.FindAsync(theId);
            Assert.NotNull(res);
            Assert.Equal(theId, res.Id);
        }

        [Fact]
        public async Task GetOneResidentUnitBySpecificationAsync_UnitNameDoesNotExist_ShouldReturnNull()
        {
            var unitName = "xxxx";
            var spec = new ResidentUnitByUnitNameSpecification(unitName);
            var res = await _fixture.ResidentUnitRepository.GetOneResidentUnitBySpecificationAsync(spec);
            Assert.Null(res);
        }

        [Fact]
        public async Task GetOneResidentUnitBySpecificationAsync_UnitNameExist_ShouldReturnResidentUnit()
        {
            var theUnitName = "RU001";
            var newResidentUnit = new ResidentUnit
            {
                Id = Guid.NewGuid(),
                UnitName = theUnitName,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            };

            await _fixture.DbContext.ResidentUnits.AddAsync(newResidentUnit);
            await _fixture.DbContext.SaveChangesAsync();
            var res = await _fixture.ResidentUnitRepository.GetOneResidentUnitBySpecificationAsync(
                new ResidentUnitByUnitNameSpecification(theUnitName));
            Assert.NotNull(res);
            Assert.Equal(theUnitName, res.UnitName);
        }

        [Fact]
        public async Task UpdateResidentUnitAsync_NewUnitName_ShouldUpdateParcel()
        {
            var theUnitName = "RU001";
            var newUnitName = "RU001-updated";
            var theId = Guid.NewGuid();
            var newResidentUnit = new ResidentUnit
            {
                Id = theId,
                UnitName = theUnitName,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            };
            var dbContext = _fixture.DbContext;
            await dbContext.ResidentUnits.AddAsync(newResidentUnit);
            await dbContext.SaveChangesAsync();
            newResidentUnit.UnitName = newUnitName;
            await _fixture.ResidentUnitRepository.UpdateResidenUnitAsync(newResidentUnit);
            var res = await dbContext.ResidentUnits.FindAsync(theId);
            Assert.NotNull(res);
            Assert.Equal(theId, res.Id);
            Assert.Equal(newUnitName, res.UnitName);

        }
    }
} 