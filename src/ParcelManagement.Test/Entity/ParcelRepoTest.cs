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
        public async Task AddParcelAsyncShouldAddToDB()
        {
            var parcel = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = "TN001",
                ResidentUnit = "RU001"
            };
            var mockSet = new Mock<DbSet<Parcel>>();
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Parcels).Returns(mockSet.Object);

            var parcelRepo = new ParcelRepository(mockContext.Object);

            await parcelRepo.AddParcelAsync(parcel);

            mockSet.Verify(m => m.AddAsync(parcel, default), Times.Once);
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task GetAllParcelShouldReturnAllParcels()
        {
            //setup parcels list
            var parcelList = new List<Parcel>
            {
                new() { Id = Guid.NewGuid(), TrackingNumber = "TN001", ResidentUnit = "RU001"},
                new() { Id = Guid.NewGuid(), TrackingNumber = "TN002", ResidentUnit = "RU002"},
                new() { Id = Guid.NewGuid(), TrackingNumber = "TN003", ResidentUnit = "RU003"}
            };

            
        }
        
    }
}