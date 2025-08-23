using ParcelManagement.Core.Entities;
using ParcelManagement.Test.Fixture;
using Xunit;

namespace ParcelManagement.Test.Service
{
    public class ParcelServiceTest(ParcelTestFixture parcelFixture) :
    IClassFixture<ParcelTestFixture>
    {
        private readonly ParcelTestFixture _parcelFixture = parcelFixture;

        [Fact]
        public async Task CheckInParcelAsync_ResidentUnitNotExist_ShouldThrowError()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await _parcelFixture.ParcelService.CheckInParcelAsync(
                trackingNumber: "TN001",
                residentUnit: "RU001", weight: 1, dimensions: "1"
            );
            });
        }

        [Fact]
        public async Task CheckInParcelAsync_ExistingParcelNumber_ShouldThrowError()
        {
            var dbContext = _parcelFixture.DbContext;
            var residentUnitName = "RU001";
            var trackingNumber = "TN001";
            var residentUnitId = Guid.NewGuid();

            // Add resident unit
            await dbContext.ResidentUnits.AddAsync(new ResidentUnit
            {
                Id = residentUnitId,
                UnitName = residentUnitName
            });
            await dbContext.SaveChangesAsync();

            // Add parcel with same tracking number
            await dbContext.Parcels.AddAsync(new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = trackingNumber,
                ResidentUnitId = residentUnitId,
                ResidentUnitDeprecated = residentUnitName,
                EntryDate = DateTimeOffset.UtcNow,
                Status = ParcelStatus.AwaitingPickup
            });
            await dbContext.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _parcelFixture.ParcelService.CheckInParcelAsync(
                    trackingNumber, residentUnitName, null, null
                );
            });
        }

        [Fact]
        public async Task CheckInParcelAsync_ResidentUnitExist_ShouldCreateParcel()
        {
            var dbContext = _parcelFixture.DbContext;
            var residentUnitName = "RU001";
            var existingResidentUnitId = Guid.NewGuid();
            var trackingNumber = "TN001";
            var existingResidentUnit = new ResidentUnit
            {
                Id = existingResidentUnitId,
                UnitName = residentUnitName
            };
            await dbContext.ResidentUnits.AddAsync(existingResidentUnit);
            await dbContext.SaveChangesAsync();

            var res = await _parcelFixture.ParcelService.CheckInParcelAsync(
                trackingNumber, residentUnitName, null, null
            );
            Assert.NotNull(res);
            Assert.Equal(existingResidentUnitId, res.ResidentUnitId);
            Assert.Equal("TN001", trackingNumber);
        }
    }
}