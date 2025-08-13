// The most common and recommended convention in the C# world is the 
// MethodName_StateUnderTest_ExpectedBehavior pattern.

using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Moq;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Specifications;
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
                var result = await parcelRepo.AddParcelAsync(parcel);

                //TODO 
                // to correct this one to use dbContext.Parcels.findAsync(id)

                //asserting the data
                Assert.NotNull(result);
                Assert.Equal(parcel.TrackingNumber, result.TrackingNumber);
                Assert.Equal(parcel.ResidentUnit, result.ResidentUnit);
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

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase(databaseName: "TestDatabase")
                    .Options;

            using (var testDbContext = new ApplicationDbContext(options))
            {
                var parcelRepo = new ParcelRepository(testDbContext);
                foreach (var p in parcelList)
                {
                    await parcelRepo.AddParcelAsync(p);
                }


                var result = await parcelRepo.GetAllParcelsAsync();

                Assert.NotNull(result);
                Assert.Equal(parcelList.Count, result.Count);
                foreach (var parcel in parcelList)
                {
                    // ! because result is nullable 
                    Assert.Contains(result, r => r!.TrackingNumber == parcel.TrackingNumber);
                }
            }

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

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "testDatabase").Options;

            using (var testDbContext = new ApplicationDbContext(options))
            {
                var parcelRepo = new ParcelRepository(testDbContext);

                await parcelRepo.AddParcelAsync(parcel);

                var result = await parcelRepo.GetParcelByIdAsync(theId);

                Assert.Equal(theId, result!.Id);
            }
        }

        [Fact]
        public async Task FindBySpecificationAsync_ByTrackingNumber_ShouldReturnParcel()
        {
            var parcelList = Enumerable.Range(1, 10)
                .Select(num => new Parcel
                {
                    Id = Guid.NewGuid(),
                    TrackingNumber = $"TN{(num < 10 ? $"00{num}" : $"0{num}")}",
                    ResidentUnit = $"RU{(num < 10 ? $"00{num}" : $"0{num}")}"
                }).ToList();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "testDatabase").Options;
            using (var testDbContext = new ApplicationDbContext(options))
            {
                var parcelRepo = new ParcelRepository(testDbContext);
                await testDbContext.Parcels.AddRangeAsync(parcelList);
                await testDbContext.SaveChangesAsync();
                var spec = new ParcelByTrackingNumberSpecification("TN001");
                var result = await parcelRepo.GetParcelsBySpecificationAsync(spec);
                Assert.NotNull(result);
                foreach (var parcel in result)
                    Assert.Contains(result, r => r!.TrackingNumber == "TN001");
            }
        }

        [Fact]
        public async Task FindBySpecification_ByResidentUnit_ShouldReturnParcels()
        {
            var residentParcels = Enumerable.Range(1, 10)
                .Select(num => new Parcel
                {
                    Id = Guid.NewGuid(),
                    TrackingNumber = $"TN{(num < 10 ? $"00{num}" : $"0{num}")}",
                    ResidentUnit = "TN001"
                }).ToList();

            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

            using (var testDbContext = new ApplicationDbContext(dbOptions))
            {
                var parcelRepo = new ParcelRepository(testDbContext);
                await testDbContext.Parcels.AddRangeAsync(residentParcels);
                await testDbContext.SaveChangesAsync();

                var spec = new ParcelsByResidentUnitSpecification("TN001");
                var result = await parcelRepo.GetParcelsBySpecificationAsync(spec);

                Assert.NotNull(result);
                Assert.Equal(residentParcels.Count, result.Count);
                foreach (var parcel in result)
                {
                    Assert.Equal("TN001", parcel!.ResidentUnit);
                }
            }
        }

    }
}