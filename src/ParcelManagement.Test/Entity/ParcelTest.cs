using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Entities;
using ParcelManagement.Infrastructure.Database;
using Xunit;

namespace ParcelManagement.Test.Entities
{
    public class ParcelTest
    {
        [Fact]
        public void ParcelValidProperties()
        {
            // Given
            var parcel = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = "TN001",
                ResidentUnitDeprecated = "RU001",
                ResidentUnitId = Guid.NewGuid(),
                EntryDate = DateTimeOffset.UtcNow,
                Status = ParcelStatus.AwaitingPickup,
                Weight = 2,
                Dimensions = "1x1x1"
            };

            parcel.Id.Should().NotBeEmpty();
            parcel.TrackingNumber.Should().Be("TN001");
            parcel.ResidentUnitDeprecated.Should().Be("RU001");
            parcel.ResidentUnitId.Should().NotBeEmpty();
            parcel.Status.Should().Be(ParcelStatus.AwaitingPickup);
            parcel.Weight.Should().Be(2);
            parcel.Dimensions.Should().Be("1x1x1");
        }

        [Fact]
        public void ParcelPropertiesShouldThrowExceptionForInvalidLength()
        {
            var parcel = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = new string('A', 51), // since length.max is 50
                ResidentUnitDeprecated = "RU002",
                ResidentUnitId = Guid.NewGuid(),
                EntryDate = DateTimeOffset.UtcNow,
                Weight = 2,
                Dimensions = "1x1x1"
            };

            Action act = () => Validator.ValidateObject(parcel, new ValidationContext(parcel), true);

            act.Should().Throw<ValidationException>();
        }
    }
}