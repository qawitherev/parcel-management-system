using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using ParcelManagement.Api.Controller.V1;
using ParcelManagement.Api.DTO;
using ParcelManagement.Api.DTO.V1;
using ParcelManagement.Core.Entities;
using ParcelManagement.Infrastructure.Database;
using ParcelManagement.Test.Integration.Misc;

namespace ParcelManagement.Test.Integration
{
    public class ParcelControllerTest : IntegrationTestBase
    {

        public ParcelControllerTest(CustomWebApplicationFactory factory): base(factory)
        {
            // nothing 
        }

        [Fact]
        public async Task GetParcelById_ParcelExist_ShouldReturnParcel()
        {
            await ResetDatabaseAsync();
            // arrange 
            var existingParcel = await Seeder.SeedParcelAsync();

            // calling endpoint 
            var response = await Client.GetAsync($"api/v1/parcel/{existingParcel.Id}");
            var fetchedParcel = await response.Content.ReadFromJsonAsync<ParcelResponseDto>(IntegrationMisc.GetJsonSerializerOptions());

            // assertion
            Assert.NotNull(fetchedParcel);
            Assert.Equal(existingParcel.Id, fetchedParcel!.Id);
        }
    }
}