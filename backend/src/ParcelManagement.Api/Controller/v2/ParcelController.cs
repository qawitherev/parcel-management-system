using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller.V2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Consumes("application/json")]
    public class ParcelController(
        IParcelService parcelService
    ) : ControllerBase
    {
        private readonly IParcelService _parcelService = parcelService;

        public async Task<IActionResult> CheckIn()
        {
            // TODO: to finish this controller 
            return Ok();
        }
    }
}