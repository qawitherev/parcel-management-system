using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ParcelManagement.Api.DTO;
using ParcelManagement.Api.DTO.V1;
using ParcelManagement.Core.Entities;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Test.Integration
{
    public class ParcelControllerTest(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task GetParcelById_NonNull_ShouldReturnParcel()
        {
            var theId = Guid.NewGuid();
            // virtually creating the data 
            using (var scope = factory.Services.CreateScope())
            {
                var dbContextUsingScope = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var parcel = new Parcel
                {
                    Id = theId,
                    TrackingNumber = "TN001",
                    ResidentUnitDeprecated = "RU001",
                    ResidentUnitId = Guid.NewGuid()
                    
                };
                await dbContextUsingScope.Parcels.AddAsync(parcel);
                await dbContextUsingScope.SaveChangesAsync();
            }

            // call the endpoint 
            var response = await _client.GetAsync($"api/parcel/GetParcelById/{theId}");

            // assert 
            response.EnsureSuccessStatusCode();
            var fetchedParcel = await response.Content.ReadFromJsonAsync<ParcelResponseDto>();
            Assert.Equal(theId, fetchedParcel!.Id);
        }


        [Fact]
        public async Task GetParcelById_NotFound_ShouldReturnNotFound()
        {
            var theId = Guid.NewGuid();

            var response = await _client.GetAsync($"api/parcel/GetParcelById/{theId}");

            // assert 
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}