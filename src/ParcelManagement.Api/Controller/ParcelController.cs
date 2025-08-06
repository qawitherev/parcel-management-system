
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.DTO;
using ParcelManagement.Core.Services;

[ApiController]
[Route("api/[controller]")]
public class ParcelController(IParcelService parcelService) : ControllerBase
{
    // [HttpPost("checkIn")]
    // public async Task<IActionResult> checkInParcel([FromBody] CheckInParcelDto dto)
    // {
        
    // }
}