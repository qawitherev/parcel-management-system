
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.DTO;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Services;

[ApiController]
[Route("api/[controller]")]
public class ParcelController(IParcelService parcelService) : ControllerBase
{

    [HttpGet("getParcelById/{id}")]
    public async Task<IActionResult> GetParcelById(Guid id)
    {
        //TODO: to check if id is null
        var parcelResult = await parcelService.GetParcelByIdAsync(id);
        return Ok(parcelResult);
    }

    [HttpPost("checkIn")]
    public async Task<IActionResult> CheckInParcel([FromBody] CheckInParcelDto dto)
    {
        
        var newParcel = new Parcel
        {
            Id = Guid.NewGuid(),
            TrackingNumber = dto.TrackingNumber,
            ResidentUnit = dto.ResidentUnit,
            Status = ParcelStatus.AwaitingPickup,
            Weight = dto.Weight ?? 0,
            Dimensions = dto.Dimensions ?? ""
        };

        await parcelService.CheckInParcelAsync(newParcel);
        return CreatedAtAction(nameof(GetParcelById), new { id = newParcel.Id }, newParcel);
    }
}