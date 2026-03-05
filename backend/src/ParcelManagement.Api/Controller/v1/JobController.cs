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
        IParcelService parcelService, 
        ISessionService sessionService
    ) : ControllerBase
    {
        [HttpPost("parcelOverstay")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> EnqueueProcessParcelOverstay()
        {
            await parcelService.WakeUpProcessParcelOverstay();
            return Accepted();
        }

        [HttpPost("cleanupSession")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> CleanupSession()
        {
            await sessionService.WakeupCleanupSession();
            return Accepted();
        }
    }
}