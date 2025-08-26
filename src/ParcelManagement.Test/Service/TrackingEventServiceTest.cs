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
        }
    }
}