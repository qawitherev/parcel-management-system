// The most common and recommended convention in the C# world is the 
// MethodName_StateUnderTest_ExpectedBehavior pattern.

using Microsoft.EntityFrameworkCore;
using Moq;
using ParcelManagement.Core.Entities;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Infrastructure.Repository;
using Xunit;

namespace ParcelManagement.Test.Repository
{
    public class ParcelRepositoryTests
    {
        [Fact]
        public async Task AddParcelAsync_ShouldAddToDB()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase") // Use an in-memory database for testing
                .Options;

            var parcel = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = "TN001",
                ResidentUnit = "RU001"
            };

            using (var testDbContext = new ApplicationDbContext(options))
            {
                var parcelRepo = new ParcelRepository(testDbContext);
                await parcelRepo.AddParcelAsync(parcel);

                //asserting the data 
                //TODO: make the code here 
            }
        }

        [Fact]
        public async Task GetAllParcelAsync_NonNull_ShouldReturnAllParcels()
        {
            //setup parcels list
            var parcelList = new List<Parcel>
            {
                new() { Id = Guid.NewGuid(), TrackingNumber = "TN001", ResidentUnit = "RU001"},
                new() { Id = Guid.NewGuid(), TrackingNumber = "TN002", ResidentUnit = "RU002"},
                new() { Id = Guid.NewGuid(), TrackingNumber = "TN003", ResidentUnit = "RU003"}
            };

            var mockSet = new Mock<DbSet<Parcel>>();
            mockSet.As<IQueryable<Parcel>>().Setup(m => m.Expression).Returns(parcelList.AsQueryable().Expression);
            mockSet.As<IQueryable<Parcel>>().Setup(m => m.Provider).Returns(parcelList.AsQueryable().Provider);
            mockSet.As<IQueryable<Parcel>>().Setup(m => m.ElementType).Returns(parcelList.AsQueryable().ElementType);

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Parcels).Returns(mockSet.Object);

            var parcelRepo = new ParcelRepository(mockContext.Object);

            var result = await parcelRepo.GetAllParcelsAsync();

            Assert.NotNull(result);
            Assert.Equal(parcelList.Count, result.Count);

        }

        [Fact]
        public async Task GetParcelByIdAsync_NotNull_ShouldReturnParcel()
        {
            var theId = Guid.NewGuid();
            var parcel = new Parcel
            {
                Id = theId,
                TrackingNumber = "TN001",
                ResidentUnit = "RU001"
            };

            var mockSet = new Mock<DbSet<Parcel>>();
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(m => m.Parcels).Returns(mockSet.Object);

            var parcelRepo = new ParcelRepository(mockContext.Object);

            var result = await parcelRepo.GetParcelByIdAsync(theId);

            Assert.Equal("TN001", parcel.TrackingNumber);
        }

    }
}