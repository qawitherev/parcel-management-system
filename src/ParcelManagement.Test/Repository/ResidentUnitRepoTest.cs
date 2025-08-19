using ParcelManagement.Core.Entities;
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
            var dbContext = _fixture.DbContext;
            var newRu = new ResidentUnit
            {
                Id = Guid.NewGuid(),
                UnitName = "RU001",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            };
        }
    }
}