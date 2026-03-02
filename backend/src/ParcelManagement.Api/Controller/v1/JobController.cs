using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Consumes("application/json")]
    public class JobController(
        IParcelService parcelService
    ) : ControllerBase
    {
        [HttpPost("parcel-overstay")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> EnqueueProcessParcelOverstay()
        {
            await parcelService.WakeUpProcessParcelOverstay();
            return Accepted();
        }
    }
}