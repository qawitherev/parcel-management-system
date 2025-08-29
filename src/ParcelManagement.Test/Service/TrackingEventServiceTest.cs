using ParcelManagement.Core.Entities;
using ParcelManagement.Test.Fixture;
using Xunit;

namespace ParcelManagement.Test.Service
{
    public class TrackingEventServiceTest : IClassFixture<TrackingEventTestFixture>
    {
        private readonly TrackingEventTestFixture _teFixture;

        public TrackingEventServiceTest(TrackingEventTestFixture teFixture)
        {
            _teFixture = teFixture;
        }

        [Fact]
        public async Task ManualEventTracking_InvalidTrackingNumber_ShouldThrowError()
        {
            await Assert.ThrowsAsync<NullReferenceException>(
                async () =>
                {
                    await _teFixture.TrackingEventService.ManualEventTracking("TN001", Guid.NewGuid(), "this is a customEvent");
                }
            );
            await _teFixture.ResetDb();
        }

        [Fact]
        public async Task ManualEventTracking_ValidTrackingNumber_ShouldReturnEntities()
        {
            var theTrackingNumber = "TN001";
            var thePerformingUser = Guid.NewGuid();
            var theCustomEvent = "The custom event";
            var parcel = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = theTrackingNumber,
                ResidentUnitId = Guid.NewGuid(),
            };
            await _teFixture.DbContext.Parcels.AddAsync(parcel);
            await _teFixture.DbContext.SaveChangesAsync();

            var (te, p) = await _teFixture.TrackingEventService.ManualEventTracking(theTrackingNumber,
                thePerformingUser, theCustomEvent
            );
            Assert.NotNull(te);
            Assert.NotNull(p);
            Assert.Equal(theCustomEvent, te.CustomEvent);
            Assert.Equal(theTrackingNumber, p.TrackingNumber);
        }
    }
}