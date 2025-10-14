using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using ParcelManagement.Api.AuthenticationAndAuthorization;
using ParcelManagement.Api.Controller.V1;
using ParcelManagement.Api.DTO;
using ParcelManagement.Api.DTO.V1;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Services;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Test.Integration.Misc;

namespace ParcelManagement.Test.Integration
{
    public class ParcelControllerTest : IntegrationTestBase
    {
        private readonly CustomWebApplicationFactory _factory;

        public ParcelControllerTest(CustomWebApplicationFactory factory): base(factory, factory.Services.GetRequiredService<ITokenService>())
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetParcelById_ParcelExist_ShouldReturnParcel()
        {
            await ResetDatabaseAsync();
            // arrange 
            var existingParcel = await Seeder.SeedParcelAsync();
            var token = await Seeder.GetLoginToken(Guid.NewGuid(), "ParcelRoomManagerTest", "ParcelRoomManager");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);


            // calling endpoint 
            var response = await Client.GetAsync($"api/v1/parcel/{existingParcel.Id}");
            var fetchedParcel = await response.Content.ReadFromJsonAsync<ParcelResponseDto>(IntegrationMisc.GetJsonSerializerOptions());

            // assertion
            Assert.NotNull(fetchedParcel);
            Assert.Equal(existingParcel.Id, fetchedParcel!.Id);
        }

        [Fact]
        public async Task GetParcelById_ParcelNotExist_ShouldReturnException()
        {
            await ResetDatabaseAsync();

            var token = await Seeder.GetLoginToken(Guid.NewGuid(), "ParcelRoomManagerTest", "ParcelRoomManager");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await Client.GetAsync($"api/v1/parcel/{Guid.NewGuid()}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CheckInParcel_ShouldReturnCheckedInParcel()
        {
            await ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var token = await Seeder.GetLoginToken(userId, "ParcelRoomManagerTest", "ParcelRoomManager");
            await Seeder.SeedForCheckIn(userId);
            var body = new CheckInParcelDto
            {
                TrackingNumber = "TN001",
                ResidentUnit = "RU001",
                Dimensions = "1x1",
                Weight = 1
            };
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var bodyStringContent = IntegrationMisc.ConvertToStringContent(body);
            var response = await Client.PostAsync($"api/v1/parcel/checkIn", bodyStringContent);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            using var scope = _factory.Services.CreateScope();
            var parcelService = scope.ServiceProvider.GetRequiredService<IParcelService>();
            var checkedInParcel = await parcelService.GetParcelByTrackingNumberAsync(body.TrackingNumber);
            Assert.NotNull(checkedInParcel);
            Assert.Equal(body.TrackingNumber, checkedInParcel.TrackingNumber);
            var fetchedParcel = await response.Content.ReadFromJsonAsync<ParcelResponseDto>(IntegrationMisc.GetJsonSerializerOptions());
            Assert.NotNull(fetchedParcel);
            Assert.Equal(body.TrackingNumber, fetchedParcel.TrackingNumber);
        }
    }
}