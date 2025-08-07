
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.DTO;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Services;

[ApiController]
[Route("api/[controller]")]
public class ParcelController(IParcelService parcelService) : ControllerBase
{
    // since we wont expose this endpoint publicly, we won't follow 
    // url pattern to give way to 
    // {"{trackingNumber}"}
    [HttpGet("GetParcelById{id}")]
    public async Task<IActionResult> GetParcelById(Guid id)
    {
        //TODO: to check if id is null
        var parcelResult = await parcelService.GetParcelByIdAsync(id);
        return Ok(parcelResult);
    }

    [HttpPost("checkIn")]
    public async Task<IActionResult> CheckInParcel([FromBody] CheckInParcelDto dto)
    {

        var newParcel = await parcelService.CheckInParcelAsync(dto.TrackingNumber, dto.ResidentUnit, dto.Weight, dto.Dimensions);
        return CreatedAtAction(nameof(GetParcelById), new { id = newParcel.Id }, newParcel);
    }

    [HttpPost("{trackingNumber}/claim")]
    public async Task<IActionResult> ClaimParcel(string trackingNumber)
    {
        await parcelService.ClaimParcelAsync(trackingNumber);
        return NoContent(); // 204
    }

    [HttpGet("{trackingNumber}")]
    public async Task<IActionResult> GetParcelByTrackingNumber(string trackingNumber)
    {
        var resultParcel = await parcelService.GetParcelByTrackingNumberAsync(trackingNumber);
        return Ok(resultParcel);
    }

    [HttpGet("awaitingPickup")]
    public async Task<IActionResult> GetParcelAwaitingPickup()
    {
        var parcelsAwaitingPickup = await parcelService.GetParcelsAwaitingPickup();
        var parcelAwaitingPickupDto = parcelsAwaitingPickup.Select(entity => new ParcelResponseDto
        {
            Id = entity!.Id,
            TrackingNumber = entity.TrackingNumber,
            ResidentUnit = entity.ResidentUnit,
            Weight = entity?.Weight,
            Dimensions = entity?.Dimensions
        });
        return Ok(parcelAwaitingPickupDto);
    }
    
}