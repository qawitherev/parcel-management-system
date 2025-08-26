using System.Runtime.CompilerServices;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
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
                residentUnit: "RU001", weight: 1, dimensions: "1",
                performedByUser: Guid.NewGuid()
            );
            });
            await _parcelFixture.ResetDb();
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
                    trackingNumber, residentUnitName, null, null,
                performedByUser: Guid.NewGuid()
                );
            });

            await _parcelFixture.ResetDb();
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
                trackingNumber, residentUnitName, null, null,
                performedByUser: Guid.NewGuid()
            );

            // checking for tracking here
            var tracking = await dbContext.TrackingEvents
                .Where(te => te.ParcelId == res.Id).FirstOrDefaultAsync();

            Assert.NotNull(res);
            Assert.Equal(existingResidentUnitId, res.ResidentUnitId);
            Assert.Equal("TN001", trackingNumber);

            Assert.NotNull(tracking);
            Assert.Equal(res.Id, tracking.ParcelId);

            await _parcelFixture.ResetDb();
        }

        [Fact]
        public async Task ClaimParcelAsync_ParcelNumberExist_ShouldThrowError()
        {
            var theTrackingNumber = "TN001";
            var existingParcel = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = theTrackingNumber
            };
            await _parcelFixture.DbContext.Parcels.AddAsync(existingParcel);
            await _parcelFixture.DbContext.SaveChangesAsync();

            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await _parcelFixture.ParcelService.CheckInParcelAsync(theTrackingNumber,
                "RU001", null, null, Guid.NewGuid()
            );
            });
            await _parcelFixture.ResetDb();
        }

        [Fact]
        public async Task ClaimParcelAsync_ResidentUnitDoesnt_Exist()
        {
            var existingParcel = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = "TN001"
            };
            await _parcelFixture.DbContext.Parcels.AddAsync(existingParcel);
            await _parcelFixture.DbContext.SaveChangesAsync();

            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await _parcelFixture.ParcelService.CheckInParcelAsync("TN001",
                "RU001", null, null, Guid.NewGuid());
            });
            await _parcelFixture.ResetDb();
        }

        [Fact]
        public async Task ClaimParcelAsync_ValidClaim_ShouldClaim()
        {
            var dbContext = _parcelFixture.DbContext;
            var theUnitName = "RU001";
            var theTrackingNumber = "TN001";
            var residentUnit = new ResidentUnit
            {
                Id = Guid.NewGuid(),
                UnitName = theUnitName
            };
            await dbContext.ResidentUnits.AddAsync(residentUnit);
            var checkedInParcel = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = theTrackingNumber,
                ResidentUnitId = residentUnit.Id,
                Status = ParcelStatus.AwaitingPickup
            };
            await dbContext.Parcels.AddAsync(checkedInParcel);
            await dbContext.SaveChangesAsync();
            await _parcelFixture.ParcelService.ClaimParcelAsync(
                theTrackingNumber, Guid.NewGuid()
            );

            var res = await dbContext.Parcels.FindAsync(checkedInParcel.Id);
            Assert.NotNull(res);
            // check for status == claimed
            Assert.Equal(ParcelStatus.Claimed, res.Status);

            // check for tracking event 
            var te = await dbContext.TrackingEvents
                .Where(e => e.ParcelId == checkedInParcel.Id).FirstOrDefaultAsync();
            Assert.NotNull(te);
            Assert.Equal(TrackingEventType.Claim, te.TrackingEventType);
            await _parcelFixture.ResetDb();
        }

        [Fact]
        public async Task GetParcelHistoriesAsync_NonExistParcel_ShouldThrowError()
        {
            
        }
    }
}